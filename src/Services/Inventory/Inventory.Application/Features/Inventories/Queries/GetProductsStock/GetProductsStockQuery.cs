namespace SmartEcommerce.Inventory.Application.Features.Inventories.Queries.GetProductsStock;

/// <summary>Batched rollup across every warehouse for each requested variation, in one query - backs the gRPC batch stock check Order Service's CreateOrder flow consumes to avoid an N+1.</summary>
public sealed record GetProductsStockQuery(IReadOnlyCollection<Guid> ProductVariationIds) : IQuery<IReadOnlyCollection<ProductVariationStockResult>>;

public sealed record ProductVariationStockResult(Guid ProductVariationId, int TotalQuantity);
