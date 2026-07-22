using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Order.Persistence.Configs;

public sealed class OrderConfig : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CustomerPhone).HasMaxLength(30).IsRequired();
        builder.Property(x => x.CustomerPhoneSearch).HasMaxLength(20).IsRequired();
        builder.Property(x => x.CustomerPhoneReverse).HasMaxLength(20).IsRequired();
        builder.Property(x => x.ShippingAddress).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.IdempotencyKey).HasMaxLength(200);
        builder.Property(x => x.CancellationReason).HasMaxLength(200);

        // TotalAmount is a computed sum over Items, not a stored column.
        builder.Ignore(x => x.TotalAmount);

        // Scoped per customer (not globally unique) so two different customers may reuse the same
        // client-generated key. Partial index (WHERE clause) so orders with no key never collide.
        builder.HasIndex(x => new { x.CustomerId, x.IdempotencyKey })
            .IsUnique()
            .HasFilter("\"idempotency_key\" IS NOT NULL");

        // Guards concurrent writers of the same Order row (CreateOrderSaga's ConfirmOrderStep vs
        // a manual CancelOrder request racing each other) - see EfUnitOfWork.ExecuteTransactionAsync,
        // which translates the resulting DbUpdateConcurrencyException into ConflictException.
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsRowVersion();

        // OrderItem has no independent identity outside its Order (no repository, no separate
        // aggregate root), so it's mapped as an EF owned collection rather than a related entity -
        // this also means owned rows are always loaded with the Order, no explicit Include() needed.
        builder.OwnsMany(x => x.Items, item =>
        {
            item.ToTable("order_items");
            item.WithOwner().HasForeignKey(i => i.OrderId);
            item.HasKey(i => i.Id);

            item.Property(i => i.ProductId).IsRequired();
            item.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
            item.Property(i => i.UnitPrice).HasColumnType("numeric(18,2)");
            item.Property(i => i.Quantity).IsRequired();
            item.Property(i => i.Discount).HasColumnType("numeric(18,2)").HasDefaultValue(0m);

            item.Ignore(i => i.LineTotal);

            item.Property(i => i.CreatedAt).HasDefaultValueSql("now()");
            item.Property(i => i.UpdatedAt).HasDefaultValueSql("now()");
        });

        // Items is a plain `{ get; private set; }` auto-property - EF invokes the private setter
        // directly, no explicit property access mode needed (see conventions/domain-coding-conventions.md#3).
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CustomerPhoneSearch);
        builder.HasIndex(x => x.CustomerPhoneReverse);
    }
}
