using NSubstitute;

using Order.Application.Abstractions.Repositories;
using Order.Application.Abstractions.Services;
using Order.Application.Features.Orders.Commands.CancelOrder;
using Order.Domain.Entities;

using Shouldly;

namespace Order.Application.Tests;

/// <summary>
/// Regression coverage for B2: cancelling a Confirmed order (stock already deducted by
/// CreateOrderSaga's DeductInventoryStep) must restock, or the deduction leaks with no
/// corresponding active order.
/// </summary>
public sealed class CancelOrderHandlerTests
{
    private static OrderEntity CreateOrder() =>
        OrderEntity.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Jane Doe", "0123456789", "123 Main St",
            [new OrderItemCreateModel(Guid.NewGuid(), "Widget", 10m, 1)]);

    private static (IOrderRepository Repo, IOutboxStore Outbox, IInventoryClientService Inventory, IUnitOfWork Uow)
        CreateSubstitutes(OrderEntity order)
    {
        var repo = Substitute.For<IOrderRepository>();
        repo.UpdateAsync(order.Id, Arg.Any<Func<OrderEntity, Task>>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Func<OrderEntity, Task>>()!(order));

        var uow = Substitute.For<IUnitOfWork>();
        uow.ExecuteTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(async ci =>
            {
                await ci.ArgAt<Func<Task>>(0)();
                return true;
            });

        return (repo, Substitute.For<IOutboxStore>(), Substitute.For<IInventoryClientService>(), uow);
    }

    [Theory]
    [InlineData(false)] // Pending
    [InlineData(true)]  // Confirmed - stock genuinely deducted by the saga
    public async Task Handle_RestocksInventory_KeyedByOrderId_AfterCancellationCommits(bool confirmed)
    {
        var order = CreateOrder();
        if (confirmed)
            order.Confirm();

        var (repo, outbox, inventory, uow) = CreateSubstitutes(order);
        var handler = new CancelOrderHandler(repo, outbox, inventory, uow);

        await handler.Handle(new CancelOrderCommand(order.Id, "Changed my mind"));

        order.Status.ShouldBe(OrderStatus.Cancelled);
        await inventory.Received(1).RestockAsync(order.Id, Arg.Any<string?>(), Arg.Any<CancellationToken>());

        // Restock only happens once the cancellation is durably committed - not before, so a
        // rejected cancellation (invalid status) can never restock a still-active order.
        Received.InOrder(() =>
        {
            uow.ExecuteTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>());
            inventory.RestockAsync(order.Id, Arg.Any<string?>(), Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_DoesNotRestock_WhenOrderIsAlreadyCancelled()
    {
        var order = CreateOrder();
        order.Cancel("First cancellation");

        var (repo, outbox, inventory, uow) = CreateSubstitutes(order);
        var handler = new CancelOrderHandler(repo, outbox, inventory, uow);

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(new CancelOrderCommand(order.Id)));

        await inventory.DidNotReceive().RestockAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DoesNotRestock_WhenOrderIsCompleted()
    {
        var order = CreateOrder();
        order.Confirm();
        order.Complete();

        var (repo, outbox, inventory, uow) = CreateSubstitutes(order);
        var handler = new CancelOrderHandler(repo, outbox, inventory, uow);

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(new CancelOrderCommand(order.Id)));

        await inventory.DidNotReceive().RestockAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }
}
