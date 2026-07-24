using Inventory.Persistence.Engine;

namespace Inventory.Persistence.Seeders;

public sealed class InventorySeeder(InventoryDbContext context)
{
    public async Task SeedAsync()
    {
        if (await context.Warehouses.AnyAsync())
            return;

        var mainWarehouse = Warehouse.Create(
            Guid.CreateVersion7(),
            "MAIN",
            "Main Warehouse",
            "Default warehouse for newly created products.");

        context.Warehouses.Add(mainWarehouse);
        await context.SaveChangesAsync();
    }
}
