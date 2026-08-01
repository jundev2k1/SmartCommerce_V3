using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Persistence.Configs;

public sealed class StockDeductionConfig : IEntityTypeConfiguration<StockDeduction>
{
    public void Configure(EntityTypeBuilder<StockDeduction> builder)
    {
        // Id is the caller-supplied deduction_id (Order Service's OrderId), never generated here.
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Status)
            .HasConversion<int>();
        builder.Property(x => x.ItemsJson)
            .HasColumnType("jsonb")
            .IsRequired();
        builder.Property(x => x.FailureCode)
            .HasMaxLength(100);
        builder.Property(x => x.Reason)
            .HasMaxLength(200);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(x => x.Status);
    }
}
