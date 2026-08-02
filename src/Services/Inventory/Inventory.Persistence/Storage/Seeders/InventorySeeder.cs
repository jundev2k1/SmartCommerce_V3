using Inventory.Domain.ValueObjects;
using Inventory.Persistence.Engine;

namespace Inventory.Persistence.Storage.Seeders;

public sealed class InventorySeeder(InventoryDbContext context)
{
    public async Task SeedAsync()
    {
        if (await context.Warehouses.AnyAsync())
            return;

        var address = Address.Create(
            country: "US",
            stateOrProvince: "CA",
            city: "Los Angeles",
            district: "",
            ward: "",
            street: "123 Main Street",
            postalCode: "90001");

        var mainWarehouse = Warehouse.Create(
            code: "MAIN",
            name: "Main Warehouse",
            type: WarehouseType.DistributionCenter,
            address: address);

        context.Warehouses.Add(mainWarehouse);
        await context.SaveChangesAsync();
    }
}
