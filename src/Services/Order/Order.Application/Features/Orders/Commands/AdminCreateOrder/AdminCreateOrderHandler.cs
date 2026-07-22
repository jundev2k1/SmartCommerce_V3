using Order.Application.Abstractions.Services;
using Order.Application.Features.Orders.Common;

namespace Order.Application.Features.Orders.Commands.AdminCreateOrder;

/// <summary>Thin - all the actual item validation/pricing/stock-check/persistence logic lives in IOrderCreationService, shared with the client-facing CreateOrderHandler. No cart lookup/diff/clear here.</summary>
public sealed class AdminCreateOrderHandler(IOrderCreationService orderCreationService)
    : ICommandHandler<AdminCreateOrderCommand, CreateOrderResponse>
{
    public Task<CreateOrderResponse> Handle(AdminCreateOrderCommand request, CancellationToken ct = default)
        => orderCreationService.CreateAsync(
            request.CustomerId, request.CustomerName, request.CustomerPhone, request.ShippingAddress, request.Items, ct);
}
