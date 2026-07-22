using Auth.Domain.Entities;

using BuildingBlock.Persistence.Ef.DbContext;
using BuildingBlock.Persistence.Ef.Inbox;
using BuildingBlock.Persistence.Ef.Outbox;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.Persistence;

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options)
    : IdentityDbContext<
        Account,
        Role,
        Guid,
        IdentityUserClaim<Guid>,
        AccountRole,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>>(options),
    IOutboxDbContext,
    IInboxDbContext
{
    // Custom Entities
    public override DbSet<Account> Users { get; set; } = null!;
    public override DbSet<Role> Roles { get; set; } = null!;
    public override DbSet<AccountRole> UserRoles { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    // Outbox and Inbox - required by IOutboxDbContext and IInboxDbContext
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;

    // Identity Claims & Logins
    //
    // AuthDbContext can't inherit BuildingBlock.Persistence.Ef.DbContext.DbContextBase - it must
    // inherit IdentityDbContext instead, for ASP.NET Core Identity's own model configuration. It
    // reuses the same shared helpers DbContextBase uses internally rather than duplicating them.
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.SuppressPendingModelChangesWarning();
    }
    public override DbSet<IdentityUserClaim<Guid>> UserClaims { get; set; } = null!;
    public override DbSet<IdentityUserLogin<Guid>> UserLogins { get; set; } = null!;
    public override DbSet<IdentityRoleClaim<Guid>> RoleClaims { get; set; } = null!;
    public override DbSet<IdentityUserToken<Guid>> UserTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyPersistenceConfigurations(typeof(AuthDbContext).Assembly);
        builder.ApplyOutboxInboxConfiguration(this);
    }
}
