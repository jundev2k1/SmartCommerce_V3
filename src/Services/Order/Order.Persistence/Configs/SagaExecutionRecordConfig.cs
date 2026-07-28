using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Order.Persistence.Reliability.Saga;

namespace Order.Persistence.Configs;

public sealed class SagaExecutionRecordConfig : IEntityTypeConfiguration<SagaExecutionRecordEntity>
{
    public void Configure(EntityTypeBuilder<SagaExecutionRecordEntity> builder)
    {
        builder.ToTable("saga_execution_records");
        builder.HasKey(x => x.SagaId);

        builder.Property(x => x.SagaId).HasMaxLength(200);
        builder.Property(x => x.SagaName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CompletedStepsJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.ContextDataJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(200);
        builder.Property(x => x.UserId).HasMaxLength(200);

        builder.HasIndex(x => x.SagaName);
        builder.HasIndex(x => x.State);
    }
}
