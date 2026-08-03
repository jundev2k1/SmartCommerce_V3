using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartEcommerce.Order.Persistence.Configs;

public sealed class OrderConfig : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        // Table
        builder.ToTable("orders");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.CancellationReason).HasMaxLength(200);

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

        // Guards concurrent writers of the same Order row (CreateOrderSaga's ConfirmOrderStep vs
        // a manual CancelOrder request racing each other) - see EfUnitOfWork.ExecuteTransactionAsync,
        // which translates the resulting DbUpdateConcurrencyException into ConflictException.
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsRowVersion();

        // Relationships
        // OrderItem has no independent identity outside its Order but is a normal related entity
        // (own table, own PK, FK back to Order) - see OrderItemConfig, which owns the other side
        // of this relationship. CustomerId/contact/shipping/IdempotencyKey now live on the 1:1
        // OrderOwner - see OrderOwnerConfig. Neither is auto-loaded with the Order the way an
        // owned collection/type was; every read path that needs them now Includes explicitly.
        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        // CreatedAt is the sort key for both admin search (OrderCriteriaDefinition) and customer
        // history (OrderHistoryCriteriaDefinition) - kept standalone for sort-only/unfiltered
        // listings, and leading a composite with Status since the two are commonly filtered+sorted
        // together (the composite also serves Status-only filters via its leftmost column, so the
        // old lone Status index was removed as redundant).
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => new { x.Status, x.CreatedAt });
    }
}
