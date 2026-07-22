using BuildingBlock.Domain.Abstractions;
using BuildingBlock.Domain.Exceptions;

namespace Inventory.Domain.Entities;

/// <summary>
/// Stock-keeping unit is (ProductVariationId, WarehouseId) - every physical unit of stock
/// belongs to a specific variation, since a Product can no longer exist without one. ProductId
/// is kept as a denormalized reference (not a duplicated quantity) purely so "total stock for a
/// product" can filter without joining back to the Product service; it is never itself a second
/// source of truth for quantity. Rollups ("total for ProductId" / "total for ProductId+VariationId")
/// are SUM(Quantity) queries over this same table, one row per (variation, warehouse) pair.
/// </summary>
public sealed class Inventory : AggregateRoot<Guid>, IAuditable
{
    public Guid ProductId { get; private set; }
    public Guid ProductVariationId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public int Quantity { get; private set; }

    private Inventory() { }

    public static Inventory Create(
        Guid id,
        Guid productId,
        Guid productVariationId,
        Guid warehouseId,
        int quantity = 0)
    {
        return new Inventory
        {
            Id = id,
            ProductId = productId,
            ProductVariationId = productVariationId,
            WarehouseId = warehouseId,
            Quantity = quantity,
        };
    }

    /// <summary>Increases stock by the given amount. Returns the delta for transaction logging.</summary>
    public void Increase(int amount)
    {
        if (amount <= 0)
            throw ExceptionFactory.InvalidRange("Quantity to increase must be greater than 0");

        Quantity += amount;
        Tourch();
    }

    /// <summary>Decreases stock by the given amount, guarding against overselling.</summary>
    public void Decrease(int amount)
    {
        if (amount <= 0)
            throw ExceptionFactory.InvalidRange("Quantity to decrease must be greater than 0");

        if (amount > Quantity)
            throw ExceptionFactory.InsufficientStock($"Insufficient stock: available {Quantity}, required {amount}");

        Quantity -= amount;
        Tourch();
    }

    /// <summary>Directly corrects stock to a known-good value (e.g. after a physical count).</summary>
    public void Adjust(int newQuantity)
    {
        if (newQuantity < 0)
            throw ExceptionFactory.InvalidRange("Quantity cannot be negative");

        Quantity = newQuantity;
        Tourch();
    }
}
