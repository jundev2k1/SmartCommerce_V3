using System.Diagnostics;
using System.Text;

using MediatR;

using Order.Application.Features.Orders.Commands.UpdateOrderOwnerInfo;
using Order.Domain.Entities;
using Order.IntegrationTests.Infrastructure;

using Shouldly;

using Xunit;
using Xunit.Abstractions;

using ApplicationException = BuildingBlock.Application.Exceptions.ApplicationException;
using DomainException = BuildingBlock.Domain.Exceptions.DomainException;
using EntityNotFoundException = BuildingBlock.Domain.Exceptions.EntityNotFoundException;

namespace Order.IntegrationTests.Concurrency;

/// <summary>
/// Diagnostic test, NOT a correctness guardrail (yet): fires two conflicting
/// UpdateOrderOwnerInfo requests at the exact same Order aggregate, at the exact same instant,
/// many times over, against a real Postgres.
///
/// Originally written against UpdateOrderCommand (wholesale item replacement), rewritten
/// 2026-07-28 to target UpdateOrderOwnerInfoCommand instead, for two independent reasons:
/// - Order Items cannot change after creation outside the Pending window - Order.UpdateItems
///   throws once Status has moved past Pending, so an item-replacement race test would need the
///   order to stay Pending for the whole test, which isn't representative of a real race (real
///   item-edit races only matter while a customer could still be editing their own still-Pending
///   cart, a narrower and less interesting window than owner-info edits, which remain valid
///   through Confirmed).
/// - Independently, Order.UpdateItems/OrderRepo.UpdateAsync has a pre-existing, unrelated bug
///   (new OrderItem entities discovered via collection-navigation fix-up get misclassified as
///   Modified instead of Added once their client-generated Guid key is non-default) that made
///   every UpdateOrder call fail unconditionally, even with zero concurrency - see
///   docs/tasks/2026-07-27/Task23_updateorder-always-fails-not-a-race-condition.md.
///
/// UpdateOrderOwnerInfo only mutates Order.Owner (CustomerPhone/ShippingAddress), never Items,
/// so it sidesteps that bug entirely while still exercising the same xmin-based optimistic
/// concurrency path (OrderConfig.cs's `.IsRowVersion()` + EfUnitOfWork's
/// DbUpdateConcurrencyException -> ConflictException translation): Order.UpdateOwnerInfo calls
/// the same Tourch() every other mutator does, which bumps the Orders row's UpdatedAt, so a
/// concurrent Owner mutation still competes for the same per-Order xmin lock even though Owner
/// is a separate 1:1 table (see OrderOwner.cs's remarks).
///
/// UpdateOrderOwnerInfo also requires Status == Confirmed (OrderWriteService.UpdateOwnerInfoAsync's
/// gate), unlike UpdateItems' Pending-only window - this test explicitly confirms the order
/// (ConfirmOrderAsync) before firing the two concurrent requests.
///
/// Expected steady state IF UpdateOrderOwnerInfo itself is healthy: every iteration produces
/// exactly one 200 and one 409 (ConflictException), and the final DB state's Owner fields
/// exactly match whichever request's intended values "won". Anything else gets logged loudly
/// and, if it constitutes actual data corruption (not just "both succeeded"), fails the test -
/// see the corruption checks at the bottom of the iteration loop.
/// </summary>
public sealed class UpdateOrderRaceConditionTests(ITestOutputHelper output) : OrderIntegrationTestBase
{
    /// <summary>Bump for more confidence (the task spec suggests up to 200); kept lower here so the suite stays fast enough to run routinely. The loop stops early the moment corruption is found, so a higher number only costs time when everything is actually clean.</summary>
    private const int Iterations = 100;

    private static readonly Guid VariationId = Guid.CreateVersion7();

    [Fact]
    public async Task ConcurrentUpdateOrderOwnerInfo_TwoConflictingReplacements_DoesNotCorruptOrderState()
    {
        var corruptionFindings = new List<string>();
        var outcomeTally = new Dictionary<(int A, int B), int>();
        var iterationsRun = 0;

        for (var iteration = 1; iteration <= Iterations; iteration++)
        {
            iterationsRun = iteration;

            // Fresh Order per iteration - per the isolation requirement, iterations must never
            // share mutable state.
            var orderId = await CreateOrderAsync(
                Guid.CreateVersion7(),
                "Race Condition Customer",
                "0900000000",
                "1 Test Street",
                new OrderItemCreateModel(VariationId, "Race Condition Variation", 10.00m, 2));

            // UpdateOrderOwnerInfo requires Status == Confirmed - see the class remarks.
            await ConfirmOrderAsync(orderId);

            // Task A and Task B: two different owner-info replacements for the exact same Order.
            var commandA = new UpdateOrderOwnerInfoCommand(orderId, "0123123123", "Address A - 123 Test Street");
            var commandB = new UpdateOrderOwnerInfoCommand(orderId, "0456456456", "Address B - 456 Test Avenue");

            await using var scopeA = CreateScope();
            await using var scopeB = CreateScope();
            var senderA = GetSender(scopeA);
            var senderB = GetSender(scopeB);

            // Barrier(2): both tasks block on SignalAndWait immediately before sending their
            // command, so the two UpdateOrderOwnerInfo requests begin as close to simultaneously
            // as the runtime allows - not two sequential awaits, and not a Sleep-based
            // approximation.
            var barrier = new Barrier(2);

            var taskA = Task.Run(() => ExecuteAsync("Task A", senderA, commandA, barrier));
            var taskB = Task.Run(() => ExecuteAsync("Task B", senderB, commandB, barrier));

            var outcomes = await Task.WhenAll(taskA, taskB);
            var outcomeA = outcomes[0];
            var outcomeB = outcomes[1];

            var finalState = await ReloadOrderAsync(orderId);

            var key = (outcomeA.StatusCode, outcomeB.StatusCode);
            outcomeTally[key] = outcomeTally.GetValueOrDefault(key) + 1;

            var iterationFindings = DetectCorruption(finalState, outcomeA, outcomeB, commandA, commandB);

            PrintIteration(iteration, outcomeA, outcomeB, finalState, iterationFindings);

            if (iterationFindings.Count > 0)
            {
                corruptionFindings.AddRange(iterationFindings.Select(f => $"Iteration {iteration}: {f}"));
                output.WriteLine($">>> Race condition / data corruption detected on iteration {iteration} - stopping early.");
                break;
            }
        }

        output.WriteLine("");
        output.WriteLine("========== Summary ==========");
        output.WriteLine($"Iterations run: {iterationsRun} / {Iterations}");
        output.WriteLine("Outcome distribution (Task A status, Task B status) -> count:");
        foreach (var (statusPair, count) in outcomeTally.OrderByDescending(kv => kv.Value))
            output.WriteLine($"  ({statusPair.A}, {statusPair.B}) -> {count}");
        output.WriteLine("==============================");

        // Per spec: do NOT fail merely because both requests succeeded (that's logged above, in
        // the tally, for a human to notice) - only fail on concrete data corruption.
        corruptionFindings.ShouldBeEmpty(
            $"Race condition produced data corruption:{Environment.NewLine}{string.Join(Environment.NewLine, corruptionFindings)}");
    }

    private static async Task<RequestOutcome> ExecuteAsync(
        string label, ISender sender, UpdateOrderOwnerInfoCommand command, Barrier barrier)
    {
        barrier.SignalAndWait();

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await sender.Send(command);
            stopwatch.Stop();
            return new RequestOutcome(label, 200, Describe(response), null, null, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new RequestOutcome(label, MapToStatusCode(ex), null, ex.GetType().Name, ex.Message, stopwatch.Elapsed);
        }
    }

    private static string Describe(UpdateOrderOwnerInfoResponse response) =>
        $"{{ OrderId = {response.OrderId}, Phone = {response.CustomerPhone}, Address = {response.ShippingAddress} }}";

    /// <summary>
    /// Mirrors BuildingBlock.Infrastructure.ExceptionHandling.ExceptionHandlerHelper's own
    /// exception->HTTP-status mapping (not reused directly, to keep this test project's
    /// dependency graph limited to Order.Application/Order.Persistence) - so the "Status" printed
    /// per task is exactly what a real client would have seen from Order.API.
    /// </summary>
    private static int MapToStatusCode(Exception ex) => ex switch
    {
        ApplicationException appEx => appEx.StatusCode,
        EntityNotFoundException => 404,
        DomainException => 400,
        _ => 500,
    };

    /// <summary>
    /// Order Items are asserted untouched first (UpdateOrderOwnerInfo must never reach them),
    /// then the owner-info corruption categories the original item-replacement version of this
    /// test checked for, translated to Owner's CustomerPhone/ShippingAddress. "Both requests
    /// succeeded" / "last write wins" is deliberately NOT itself treated as corruption - logged
    /// prominently elsewhere - but a 200 response whose claimed values never actually persisted
    /// (a lost update silently masquerading as success) is.
    /// </summary>
    private static List<string> DetectCorruption(
        OrderSnapshot? finalState,
        RequestOutcome outcomeA,
        RequestOutcome outcomeB,
        UpdateOrderOwnerInfoCommand commandA,
        UpdateOrderOwnerInfoCommand commandB)
    {
        var findings = new List<string>();

        if (finalState is null)
        {
            findings.Add("Order could not be reloaded after both requests completed (missing entirely).");
            return findings;
        }

        var order = finalState.Order;

        if (order.Items.Count != 1 || order.Items.Single().Quantity != 2)
        {
            findings.Add(
                "Order Items changed even though UpdateOrderOwnerInfo must never touch them - " +
                $"got {order.Items.Count} item(s): {string.Join(", ", order.Items.Select(i => $"{i.ProductId}={i.Quantity}"))}.");
        }

        var owner = order.Owner;
        var matchesA = owner.OwnerPhone == commandA.CustomerPhone && owner.ShippingAddress == commandA.ShippingAddress;
        var matchesB = owner.OwnerPhone == commandB.CustomerPhone && owner.ShippingAddress == commandB.ShippingAddress;

        // Only meaningful when at least one request actually reported success - if both failed,
        // the correct/expected final state is whatever existed before either ran (neither A's nor
        // B's replacement), which is NOT corruption.
        var anySucceeded = outcomeA.StatusCode == 200 || outcomeB.StatusCode == 200;
        if (anySucceeded && !matchesA && !matchesB)
        {
            findings.Add(
                "A request reported success, but the persisted owner info matches neither Task A's nor " +
                $"Task B's intended values - got phone={owner.OwnerPhone}/address={owner.ShippingAddress}, " +
                $"expected A=phone={commandA.CustomerPhone}/address={commandA.ShippingAddress} or " +
                $"B=phone={commandB.CustomerPhone}/address={commandB.ShippingAddress}.");
        }

        // A "successful" response's reported values must match what actually persisted -
        // otherwise the client was told something that isn't true, a sign that request's write
        // was silently overwritten/lost instead of surfacing as a 409.
        foreach (var (outcome, command) in new[] { (outcomeA, commandA), (outcomeB, commandB) })
        {
            if (outcome.StatusCode != 200)
                continue;

            var thisRequestWon = owner.OwnerPhone == command.CustomerPhone && owner.ShippingAddress == command.ShippingAddress;
            if (!thisRequestWon)
            {
                findings.Add(
                    $"{outcome.Label} reported success (200) for phone={command.CustomerPhone}/address={command.ShippingAddress}, " +
                    $"but the persisted order shows phone={owner.OwnerPhone}/address={owner.ShippingAddress} - a lost update.");
            }
        }

        return findings;
    }

    private void PrintIteration(
        int iteration,
        RequestOutcome outcomeA,
        RequestOutcome outcomeB,
        OrderSnapshot? finalState,
        IReadOnlyCollection<string> findings)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine($"Iteration {iteration}");
        sb.AppendLine();
        AppendOutcome(sb, outcomeA);
        sb.AppendLine();
        AppendOutcome(sb, outcomeB);
        sb.AppendLine();

        if (finalState is null)
        {
            sb.AppendLine("Final state: ORDER NOT FOUND");
        }
        else
        {
            sb.AppendLine($"Final Version (xmin): {finalState.Version}");
            sb.AppendLine($"Final Owner: Phone={finalState.Order.Owner.OwnerPhone}, Address={finalState.Order.Owner.ShippingAddress}");
            sb.AppendLine("Final Items (must be unchanged by UpdateOrderOwnerInfo):");
            foreach (var item in finalState.Order.Items.OrderBy(i => i.ProductId))
                sb.AppendLine($"  - Variation {item.ProductId}: Quantity={item.Quantity}");
        }

        if (findings.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("CORRUPTION DETECTED:");
            foreach (var finding in findings)
                sb.AppendLine($"  ! {finding}");
        }

        output.WriteLine(sb.ToString());
    }

    private static void AppendOutcome(StringBuilder sb, RequestOutcome outcome)
    {
        sb.AppendLine($"{outcome.Label}:");
        sb.AppendLine($"Status: {outcome.StatusCode}");
        sb.AppendLine($"Elapsed: {outcome.Elapsed.TotalMilliseconds:F1}ms");
        if (outcome.ResponseBody is not null)
        {
            sb.AppendLine($"Response: {outcome.ResponseBody}");
        }
        else
        {
            sb.AppendLine($"Exception: {outcome.ExceptionType}");
            sb.AppendLine($"Message: {outcome.ExceptionMessage}");
        }
    }

    private sealed record RequestOutcome(
        string Label,
        int StatusCode,
        string? ResponseBody,
        string? ExceptionType,
        string? ExceptionMessage,
        TimeSpan Elapsed);
}
