using Order.Application.Features.Orders.Common;

namespace Order.Application.Abstractions.Services;

/// <summary>
/// Shared by CreateOrder and UpdateOrder: resolves requested variations against the locally
/// synced product catalog (server-trusted pricing, never the request), then confirms stock
/// availability via Inventory's gRPC query.
/// </summary>
public interface IOrderItemPreparationService
{
    Task<OrderItemCreateModel[]> PrepareAsync(IReadOnlyCollection<OrderItemRequest> items, CancellationToken ct = default);
}
