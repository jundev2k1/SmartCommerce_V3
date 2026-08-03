using SmartEcommerce.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartEcommerce.Auth.Persistence.Configs;

public sealed class AccountRoleConfig : IEntityTypeConfiguration<AccountRole>
{
    public void Configure(EntityTypeBuilder<AccountRole> builder)
    {
        builder.ToTable("user_roles");
        builder.HasKey(ar => new { ar.UserId, ar.RoleId });

        builder.HasOne(ar => ar.Account)
            .WithMany(a => a.AccountRoles)
            .HasForeignKey(ar => ar.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ar => ar.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ar => ar.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
