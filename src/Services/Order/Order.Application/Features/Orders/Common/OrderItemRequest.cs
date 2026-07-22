namespace Order.Application.Features.Orders.Common;

/// <summary>Shared by CreateOrder and UpdateOrder - see OrderItemPreparationService.</summary>
public sealed record OrderItemRequest(Guid ProductId, Guid VariationId, int Quantity, decimal Discount = 0m);
