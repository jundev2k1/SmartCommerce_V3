using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;
using BuildingBlock.Contract.Events.Order;

using Mapster;

using Order.Application.Abstractions.Persistence.OrderProductCatalogs;
using Order.Application.Abstractions.Persistence.Orders;
using Order.Application.Abstractions.Services;
using Order.Application.Features.Orders.DTOs;

namespace Order.Application.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderHandler(
    ICurrentUserService currentUser,
    IOrderProductCatalogReadService catalogReadService,
    IOrderReadService orderReadService,
    IOrderWriteService orderWriteService,
    ICartService cartService,
    IUnitOfWork uow,
    IInventoryClientService inventoryClient,
    IOutboxStore outboxStore) : ICommandHandler<CreateOrderCommand, CreateOrderResponse>
{
    public async Task<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken ct = default)
    {
        // Validate idempotency key and check for existing order with the same key
        var idempotencyId = currentUser.GetIdempotencyKey()
            ?? throw new BadRequestException(MessageCode.InvalidInput, "Missing currelation ID from Header.");
        await CheckExistingIdempotentOrderAsync(request.Owner.OwnerId, idempotencyId, ct);

        // Check if order items match the user's cart (if not admin-created)
        // And ensure stock availability
        var isAdminCreated = request.CreatedById.HasValue;
        var orderItemInfos = await CheckAndGetOrderItemsAsync(
            isAdminCreated,
            request.Owner.OwnerId,
            request.Items,
            ct);

        var response = await StartProcessingAsync(
            idempotencyId,
            request.Owner,
            request.ShippingInfo,
            orderItemInfos,
            request.CreatedById,
            ct);

        // Clear the user's cart if the order was created by the user (not an admin)
        // if (!request.CreatedById.HasValue)
        //    await cartService.ClearCartAsync(request.Owner.OwnerId, ct);

        return response;
    }

    #region Check existing idempotent order
    private async Task CheckExistingIdempotentOrderAsync(
        Guid ownerId,
        string idempotencyKey,
        CancellationToken ct)
    {
        var duplicate = await orderReadService.GetByIdempotencyKeyAsync(ownerId, idempotencyKey, ct);
        if (duplicate is not null)
            throw new ConflictException(
                $"OrderId: {duplicate.Id} - Order Number: {duplicate.OrderNumber}. " +
                "An order with the same idempotency key already exists. " +
                "If you are retrying a previous request, please use the same idempotency key.");
    }
    #endregion

    #region Validation order items
    private async Task<OrderItemInfos> CheckAndGetOrderItemsAsync(
        bool isAdminCreated,
        Guid ownerId,
        OrderItemRequestDto[] items, CancellationToken ct)
    {
        if (!isAdminCreated)
        {
            return await EnsureCartMatchesAsync(ownerId, items, ct);
        }
        else
        {
            return await EnsureItemsAreInStockAsync(items, ct);
        }
    }

    private async Task<OrderItemInfos> EnsureCartMatchesAsync(
        Guid customerId,
        OrderItemRequestDto[] items,
        CancellationToken ct)
    {
        var (catalogs, cart) = await cartService.GetCartAsync(customerId, ct);

        var requested = items.Select(i => (i.VariationId, i.Quantity)).ToHashSet();
        var current = cart.Items.Select(i => (i.VariationId, i.Quantity)).ToHashSet();

        if (!requested.SetEquals(current))
            throw new ConflictException(
                "Your cart has changed since it was last loaded (an item's quantity, availability, " +
                "or presence differs). Call GET /cart to refresh, then resubmit.");

        return new OrderItemInfos(catalogs, cart.Items.Adapt<OrderItemRequestDto[]>());
    }

    private async Task<OrderItemInfos> EnsureItemsAreInStockAsync(
        OrderItemRequestDto[] items,
        CancellationToken ct)
    {
        var variationIds = items
            .Select(i => i.VariationId)
            .ToArray();
        var catalogEntries = await catalogReadService.GetByVariantionIdsAsync(variationIds, ct);
        if (variationIds.Length != catalogEntries.Length)
            throw new ConflictException(
                "Your cart has changed since it was last loaded " +
                "(an item's quantity, availability, or presence differs).");

        var stockByVariation = await inventoryClient.GetAvailableStockBatchAsync(variationIds, ct);
        if (stockByVariation.Count != variationIds.Length)
            throw new ConflictException(
                "One or more items in your order are no longer available (deleted or deactivated).");

        if (stockByVariation.Values.Any(stock => stock < 1))
            throw new InsufficientAmountException("One or more items in your order are out of stock.");

        return new OrderItemInfos(catalogEntries, items);
    }
    #endregion

    #region Create order and publish event bus
    private async Task<CreateOrderResponse> StartProcessingAsync(
        string idempotencyId,
        OrderOwnerRequestDto owner,
        OrderShippingInfoRequestDto shippingInfo,
        OrderItemInfos itemInfo,
        Guid? createdById,
        CancellationToken ct)
    {
        var correlationId = currentUser.GetCorrelationId();

        // Create order entity and persist it
        var order = CreateOrderEntity(
            idempotencyId,
            owner,
            shippingInfo,
            itemInfo,
            createdById);

        // Create order created items to publish to event bus
        var orderCreatedItems = order.Items
            .Select(i => new OrderCreatedItem(
                i.ProductId,
                i.VariationId,
                i.Name,
                i.Quantity.Value,
                i.UnitPrice.Value))
            .ToArray();

        // Persist data and create outbox
        OrderCreatedIntegrationEvent eventBus = null!;
        await uow.ExecuteTransactionAsync(async () =>
        {
            await orderWriteService.CreateAsync(order, ct);
            eventBus = new OrderCreatedIntegrationEvent(
                order.Id,
                owner.OwnerId,
                orderCreatedItems,
                order.TotalAmount,
                correlationId);
            await outboxStore.EnqueueAsync(eventBus, ct);
        }, ct: ct);

        // TODO: Dispatch event directly
        // ...
        // ...
        // ...

        return new CreateOrderResponse(
            order.Id,
            order.OrderNumber.Value);
    }

    private OrderEntity CreateOrderEntity(
        string idempotencyId,
        OrderOwnerRequestDto owner,
        OrderShippingInfoRequestDto shippingInfo,
        OrderItemInfos itemInfo,
        Guid? createdById)
    {
        // Create order aggregate root entity
        var order = OrderEntity.Create(idempotencyId, createdById);

        // Create and set order owner
        SetOrderOwner(order, owner);

        // Create and set order items
        SetOrderItems(order, itemInfo);

        // Create and set order shipping
        SetOrderShipping(order, shippingInfo);

        return order;
    }

    private static void SetOrderOwner(
        OrderEntity order,
        OrderOwnerRequestDto owner)
    {
        var orderOwner = OrderOwner.Create(
            order.Id,
            owner.OwnerId,
            owner.OwnerName,
            owner.OwnerEmail,
            owner.OwnerPhone);
        order.SetOwner(orderOwner);
    }

    private static void SetOrderShipping(
        OrderEntity order,
        OrderShippingInfoRequestDto shipping)
    {
        var shippingFee = 0M;
        var orderShipping = OrderShipping.Create(
            order.Id,
            shipping.ReceiverName,
            shipping.ReceiverPhone,
            shipping.ShippingAddress,
            shipping.ShippingMethod,
            Money.Create(shippingFee),
            shipping.Note);
        order.SetShipping(orderShipping);
    }

    private static void SetOrderItems(OrderEntity order, OrderItemInfos itemInfo)
    {
        OrderItem MapToOrderItem(OrderItemRequestDto item)
        {
            var catalog = itemInfo.Catalogs.FirstOrDefault(c => c.Id == item.VariationId)
                ?? throw new NotFoundException(nameof(item.VariationId), item.VariationId);

            return OrderItem.Create(
                order.Id,
                item.ProductId,
                item.VariationId,
                catalog.Name,
                catalog.Price,
                Quantity.Create(item.Quantity));
        }

        var orderItems = itemInfo.Items
            .Select(MapToOrderItem)
            .ToArray();
        order.SetOrderItems(orderItems);
    }
    #endregion

    private record OrderItemInfos(OrderProductCatalog[] Catalogs, OrderItemRequestDto[] Items);
}
