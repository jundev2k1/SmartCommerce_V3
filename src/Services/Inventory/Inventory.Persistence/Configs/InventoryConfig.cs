using BuildingBlock.Persistence.Ef.Configurations;
using Inventory.Domain.Entities.Inventories;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Persistence.Configs;

public sealed class InventoryConfig : IEntityTypeConfiguration<InventoryStock>
{
    public void Configure(EntityTypeBuilder<InventoryStock> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.VariantId)
            .IsRequired();

        builder.Property(x => x.WarehouseId)
            .IsRequired();

        builder.Property(x => x.Available)
            .IsRequired();

        builder.HasIndex(x => new { x.VariantId, x.WarehouseId })
            .IsUnique();

        builder.HasIndex(x => x.ProductId);

        builder.HasIndex(x => x.WarehouseId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
