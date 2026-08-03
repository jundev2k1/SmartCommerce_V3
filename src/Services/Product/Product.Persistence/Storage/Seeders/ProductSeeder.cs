using SmartEcommerce.Product.Persistence.Engine;

namespace SmartEcommerce.Product.Persistence.Storage.Seeders;

public sealed class ProductSeeder(ProductDbContext context)
{
    public async Task SeedAsync()
    {
        if (await context.ProductCategories.AnyAsync())
            return;

        var uncategorized = ProductCategory.Create(
            Guid.CreateVersion7(),
            CategoryCode.Create("UNCATEGORIZED"),
            "Uncategorized",
            "Default category for products without an assigned category.");

        context.ProductCategories.Add(uncategorized);
        await context.SaveChangesAsync();
    }
}
