using BuildingBlock.Domain.Abstractions;

using Inventory.Domain.Enums;

namespace Inventory.Domain.Entities;

public sealed class Warehouse : AggregateRoot<Guid>, IAuditable
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public WarehouseStatus Status { get; private set; }

    private Warehouse() { }

    public static Warehouse Create(
        Guid id,
        string code,
        string name,
        string address,
        WarehouseStatus status = WarehouseStatus.Active)
    {
        return new Warehouse
        {
            Id = id,
            Code = code,
            Name = name,
            Address = address,
            Status = status,
        };
    }

    public void UpdateDetails(string name, string address)
    {
        Name = name;
        Address = address;
        Tourch();
    }

    public void Activate()
    {
        Status = WarehouseStatus.Active;
        Tourch();
    }

    public void Deactivate()
    {
        Status = WarehouseStatus.Inactive;
        Tourch();
    }
}
