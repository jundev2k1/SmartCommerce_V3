using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Order.Persistence.Configs;

public sealed class OrderItemConfig : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        // Table
        builder.ToTable("order_items");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId)
            .IsRequired();
        builder.Property(x => x.ProductId)
            .IsRequired();
        builder.Property(x => x.ProductName)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(x => x.UnitPrice)
            .HasColumnType("numeric(18,2)");
        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("now()");

        // Relationships
        // OrderItem has no Order navigation property (see NovaCore.Order.Domain remarks - Order.Items is
        // one-directional), so the "one" side is a shadow reference rather than x => x.Order.
        builder.HasOne<OrderEntity>()
            .WithMany(o => o.Items)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.OrderId);
    }
}
