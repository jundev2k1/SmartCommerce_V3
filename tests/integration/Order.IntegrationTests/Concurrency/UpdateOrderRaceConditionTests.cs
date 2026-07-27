using System.Diagnostics;
using System.Text;

using MediatR;

using Order.Application.Features.Orders.Commands.UpdateOrder;
using Order.Application.Features.Orders.Common;
using Order.Domain.Entities;
using Order.IntegrationTests.Infrastructure;

using Shouldly;

using Xunit;
using Xunit.Abstractions;

using ApplicationException = BuildingBlock.Application.Exceptions.ApplicationException;
using DomainException = BuildingBlock.Domain.Exceptions.DomainException;
using EntityNotFoundException = BuildingBlock.Domain.Exceptions.EntityNotFoundException;
using OrderEntity = Order.Domain.Entities.Order;

namespace Order.IntegrationTests.Concurrency;

/// <summary>
/// Diagnostic test, NOT a correctness guardrail (yet): fires two conflicting UpdateOrder requests
/// at the exact same Order aggregate, at the exact same instant, many times over, against a real
/// Postgres. The point is to empirically prove whether the existing xmin-based optimistic
/// concurrency protection (see OrderConfig.cs's `.IsRowVersion()` + EfUnitOfWork's
/// DbUpdateConcurrencyException -> ConflictException translation) actually holds up under real
/// concurrent load - not to assert it does. See docs/tasks (business-requirements audit) Task 2:
/// the client-facing side of order concurrency (round-tripping a version token through the API
/// contract) is a known separate gap; this test only probes the server-side DB-level protection.
///
/// Expected steady state IF UpdateOrder itself were healthy: every iteration produces exactly one
/// 200 and one 409 (ConflictException), and the final DB state exactly matches whichever
/// request's intended item list "won". Anything else gets logged loudly and, if it constitutes
/// actual data corruption (not just "both succeeded"), fails the test - see the corruption checks
/// at the bottom of the iteration loop.
///
/// AS OF 2026-07-27 this suite instead observes "409, 409" on every single iteration - not
/// because of the concurrency race this test targets, but because of an unrelated, pre-existing
/// bug that makes UpdateOrder fail unconditionally, even with zero concurrent requests (see
/// OrderIntegrationTestBase's remarks and docs/tasks/2026-07-27/Task23_updateorder-always-fails-not-a-race-condition.md).
/// This test still runs and still does its job (log everything, only fail on actual corruption) -
/// it just can't yet exercise the "one wins, one conflicts" scenario it was built for until that
/// bug is fixed. Re-run this suite once Task 23 lands to get the intended signal.
/// </summary>
public sealed class UpdateOrderRaceConditionTests(ITestOutputHelper output) : OrderIntegrationTestBase
{
    /// <summary>Bump for more confidence (the task spec suggests up to 200); kept lower here so the suite stays fast enough to run routinely. The loop stops early the moment corruption is found, so a higher number only costs time when everything is actually clean.</summary>
    private const int Iterations = 100;

    private static readonly Guid VariationV1 = Guid.CreateVersion7();
    private static readonly Guid VariationV2 = Guid.CreateVersion7();
    private static readonly Guid VariationV3 = Guid.CreateVersion7();
    private static readonly Guid ProductId = Guid.CreateVersion7();

    [Fact]
    public async Task ConcurrentUpdateOrder_TwoConflictingReplacements_DoesNotCorruptOrderState()
    {
        await SeedCatalogAsync(
            (VariationV1, ProductId, "Variation V1", "SKU-V1", 10.00m),
            (VariationV2, ProductId, "Variation V2", "SKU-V2", 20.00m),
            (VariationV3, ProductId, "Variation V3", "SKU-V3", 30.00m));

        var corruptionFindings = new List<string>();
        var outcomeTally = new Dictionary<(int A, int B), int>();
        var iterationsRun = 0;

        for (var iteration = 1; iteration <= Iterations; iteration++)
        {
            iterationsRun = iteration;

            // Fresh Order per iteration - per the isolation requirement, iterations must never
            // share mutable state, only the (read-only, already-seeded) catalog rows above.
            var orderId = await CreateOrderAsync(
                Guid.CreateVersion7(),
                "Race Condition Customer",
                "0900000000",
                "1 Test Street",
                new OrderItemCreateModel(VariationV1, "Variation V1", 10.00m, 3),
                new OrderItemCreateModel(VariationV2, "Variation V2", 20.00m, 4));

            // Task A: bump quantities on the existing two items.
            var commandA = new UpdateOrderCommand(orderId, [
                new OrderItemRequest(VariationV1, VariationV1, 10),
                new OrderItemRequest(VariationV2, VariationV2, 5),
            ]);
            var expectedA = new[] { (VariationV1, 10), (VariationV2, 5) };

            // Task B: drop V2, add V3 - overlapping (V1) but structurally different from A.
            var commandB = new UpdateOrderCommand(orderId, [
                new OrderItemRequest(VariationV1, VariationV1, 2),
                new OrderItemRequest(VariationV3, VariationV3, 7),
            ]);
            var expectedB = new[] { (VariationV1, 2), (VariationV3, 7) };

            await using var scopeA = CreateScope();
            await using var scopeB = CreateScope();
            var senderA = GetSender(scopeA);
            var senderB = GetSender(scopeB);

            // Barrier(2): both tasks block on SignalAndWait immediately before sending their
            // command, so the two UpdateOrder requests begin as close to simultaneously as the
            // runtime allows - not two sequential awaits, and not a Sleep-based approximation.
            var barrier = new Barrier(2);

            var taskA = Task.Run(() => ExecuteAsync("Task A", senderA, commandA, barrier));
            var taskB = Task.Run(() => ExecuteAsync("Task B", senderB, commandB, barrier));

            var outcomes = await Task.WhenAll(taskA, taskB);
            var outcomeA = outcomes[0];
            var outcomeB = outcomes[1];

            var finalState = await ReloadOrderAsync(orderId);

            var key = (outcomeA.StatusCode, outcomeB.StatusCode);
            outcomeTally[key] = outcomeTally.GetValueOrDefault(key) + 1;

            var iterationFindings = DetectCorruption(finalState, outcomeA, outcomeB, expectedA, expectedB);

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
        string label, ISender sender, UpdateOrderCommand command, Barrier barrier)
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

    private static string Describe(UpdateOrderResponse response) =>
        $"{{ OrderId = {response.OrderId}, TotalAmount = {response.TotalAmount}, Status = {response.Status} }}";

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
    /// The four corruption categories the task spec calls out explicitly. "Both requests
    /// succeeded" / "last write wins" is deliberately NOT included here - logged prominently
    /// elsewhere, but not itself treated as corruption per the spec's assertion guidance.
    /// </summary>
    private static List<string> DetectCorruption(
        OrderSnapshot? finalState,
        RequestOutcome outcomeA,
        RequestOutcome outcomeB,
        (Guid VariationId, int Quantity)[] expectedA,
        (Guid VariationId, int Quantity)[] expectedB)
    {
        var findings = new List<string>();

        if (finalState is null)
        {
            findings.Add("Order could not be reloaded after both requests completed (missing entirely).");
            return findings;
        }

        var order = finalState.Order;

        var duplicateVariations = order.Items
            .GroupBy(i => i.ProductId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();
        if (duplicateVariations.Length > 0)
            findings.Add($"Duplicate OrderItems for variation(s): {string.Join(", ", duplicateVariations)}.");

        var invalidQuantityItems = order.Items.Where(i => i.Quantity <= 0).ToArray();
        if (invalidQuantityItems.Length > 0)
        {
            findings.Add(
                $"Item(s) with non-positive quantity: {string.Join(", ", invalidQuantityItems.Select(i => $"{i.ProductId}={i.Quantity}"))}.");
        }

        // Compared as full (variationId, quantity) pairs, not just variation ids - two different
        // replacements can share the same variation id set (as A and B deliberately do, both
        // touching V1) with different quantities, so id-only comparison can't tell "A won" apart
        // from "neither won, the pre-existing item happens to share A's variation ids".
        var finalPairs = order.Items.Select(i => (i.ProductId, i.Quantity)).OrderBy(p => p.ProductId).ToArray();
        var matchesA = finalPairs.SequenceEqual(expectedA.OrderBy(p => p.VariationId));
        var matchesB = finalPairs.SequenceEqual(expectedB.OrderBy(p => p.VariationId));

        // Only meaningful when at least one request actually reported success - if both failed,
        // the correct/expected final state is whatever existed before either ran (neither A's nor
        // B's replacement), which is NOT corruption.
        var anySucceeded = outcomeA.StatusCode == 200 || outcomeB.StatusCode == 200;
        if (anySucceeded && !matchesA && !matchesB)
        {
            findings.Add(
                "A request reported success, but the final item set matches neither Task A's nor Task B's " +
                $"intended replacement - got [{string.Join(", ", finalPairs)}], expected A=[{string.Join(", ", expectedA)}] or B=[{string.Join(", ", expectedB)}].");
        }

        // A "successful" response's reported TotalAmount must match what actually persisted -
        // otherwise the client was told something that isn't true, a sign the response was built
        // from state that got silently overwritten after the fact.
        foreach (var (outcome, expectedPairs) in new[] { (outcomeA, expectedA), (outcomeB, expectedB) })
        {
            if (outcome.StatusCode != 200)
                continue;

            var thisRequestWon = finalPairs.SequenceEqual(expectedPairs.OrderBy(p => p.VariationId));
            if (thisRequestWon && Math.Abs(order.TotalAmount - ExtractReportedTotal(outcome)) > 0.0001m)
            {
                findings.Add(
                    $"{outcome.Label} reported success with a TotalAmount that doesn't match the persisted order " +
                    $"(reported {ExtractReportedTotal(outcome)}, actual {order.TotalAmount}).");
            }
        }

        return findings;
    }

    private static decimal ExtractReportedTotal(RequestOutcome outcome)
    {
        // ResponseBody is the Describe(...) string built at call time - reparsing it is simpler
        // than threading a second typed value through RequestOutcome for one diagnostic check.
        if (outcome.ResponseBody is null)
            return decimal.MinValue;

        var marker = "TotalAmount = ";
        var start = outcome.ResponseBody.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        var end = outcome.ResponseBody.IndexOf(',', start);
        return decimal.Parse(outcome.ResponseBody[start..end]);
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
            sb.AppendLine("Final Items:");
            foreach (var item in finalState.Order.Items.OrderBy(i => i.ProductId))
                sb.AppendLine($"  - Variation {item.ProductId}: Quantity={item.Quantity}, UnitPrice={item.UnitPrice}, Discount={item.Discount}, LineTotal={item.LineTotal}");
            sb.AppendLine($"TotalAmount: {finalState.Order.TotalAmount}");
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
