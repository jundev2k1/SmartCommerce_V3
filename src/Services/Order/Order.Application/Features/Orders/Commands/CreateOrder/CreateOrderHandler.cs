using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;

using Order.Application.Abstractions.Services;
using Order.Application.Features.Orders.Common;

namespace Order.Application.Features.Orders.Commands.CreateOrder;

/// <summary>
/// Client-facing CreateOrder: identity is always the caller's JWT claim, never trusted from the
/// request body (see AdminCreateOrderHandler for the role-gated on-behalf-of path). Before
/// creating anything, the submitted items are diffed against the caller's own server-side cart
/// (see ICartService) - a mismatch means the client's local view of its cart is stale (an item's
/// quantity, availability, or presence changed since it was last fetched), so the create is
/// rejected rather than silently proceeding against a cart the user never actually confirmed. Item
/// validation/pricing/stock-check and persistence are delegated to IOrderCreationService, shared
/// with AdminCreateOrderHandler. On success the cart is cleared, mirroring normal checkout behavior.
/// </summary>
public sealed class CreateOrderHandler(
    ICurrentUserService currentUser,
    ICartService cartService,
    IOrderCreationService orderCreationService) : ICommandHandler<CreateOrderCommand, CreateOrderResponse>
{
    public async Task<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken ct = default)
    {
        var customerId = currentUser.GetUserId() ?? throw new ForbiddenException();

        await EnsureCartMatchesAsync(customerId, request.Items, ct);

        var response = await orderCreationService.CreateAsync(
            customerId, request.CustomerName, request.CustomerPhone, request.ShippingAddress, request.Items, ct);

        await cartService.ClearCartAsync(customerId, ct);

        return response;
    }

    private async Task EnsureCartMatchesAsync(Guid customerId, IReadOnlyCollection<OrderItemRequest> items, CancellationToken ct)
    {
        var cart = await cartService.GetCartAsync(customerId, ct);

        var requested = items.Select(i => (i.VariationId, i.Quantity)).ToHashSet();
        var current = cart.Items.Select(i => (i.VariationId, i.Quantity)).ToHashSet();

        if (!requested.SetEquals(current))
            throw new ConflictException(
                "Your cart has changed since it was last loaded (an item's quantity, availability, " +
                "or presence differs). Call GET /cart to refresh, then resubmit.");
    }
}
