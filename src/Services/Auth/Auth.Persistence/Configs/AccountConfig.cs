using SmartEcommerce.Auth.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartEcommerce.Auth.Persistence.Configs;

public sealed class AccountConfig : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("users");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.UserName)
            .IsRequired()
            .HasMaxLength(256);

        // Login (AccountReadService.GetByEmailAsync) filters the raw Email column - ASP.NET
        // Identity's own index only covers the normalized_email column, so this predicate was
        // otherwise served by a sequential scan.
        builder.HasIndex(a => a.Email);

        builder.HasMany(a => a.AccountRoles)
            .WithOne(ar => ar.Account)
            .HasForeignKey(ar => ar.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
