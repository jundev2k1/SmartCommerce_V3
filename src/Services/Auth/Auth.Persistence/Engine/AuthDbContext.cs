using SmartEcommerce.Auth.Domain.Entities.Accounts;
using SmartEcommerce.Auth.Domain.Entities.Invitations;
using SmartEcommerce.Auth.Domain.Entities.Permissions;
using SmartEcommerce.Auth.Domain.Entities.Positions;
using SmartEcommerce.Auth.Domain.Entities.Roles;
using SmartEcommerce.Auth.Domain.Entities.TokenBlacklists;

using SmartEcommerce.BuildingBlock.Persistence.Ef.DbContext;
using SmartEcommerce.BuildingBlock.Persistence.Ef.Inbox;
using SmartEcommerce.BuildingBlock.Persistence.Ef.Outbox;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SmartEcommerce.Auth.Persistence.Engine;

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
    public DbSet<Session> Sessions { get; set; } = null!;
    public DbSet<Device> Devices { get; set; } = null!;
    public DbSet<LoginHistory> LoginHistories { get; set; } = null!;
    public DbSet<PasswordHistory> PasswordHistories { get; set; } = null!;
    public DbSet<MfaMethod> MfaMethods { get; set; } = null!;
    public DbSet<MfaBackupCode> MfaBackupCodes { get; set; } = null!;
    public DbSet<ExternalIdentity> ExternalIdentities { get; set; } = null!;
    public DbSet<AccountPosition> AccountPositions { get; set; } = null!;
    public DbSet<AccountPermission> AccountPermissions { get; set; } = null!;

    public DbSet<Position> Positions { get; set; } = null!;
    public DbSet<PositionRole> PositionRoles { get; set; } = null!;
    public DbSet<PositionTranslation> PositionTranslations { get; set; } = null!;

    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<RoleTranslation> RoleTranslations { get; set; } = null!;

    public DbSet<PermissionGroup> PermissionGroups { get; set; } = null!;
    public DbSet<PermissionGroupTranslation> PermissionGroupTranslations { get; set; } = null!;
    public DbSet<PermissionDefinition> PermissionDefinitions { get; set; } = null!;
    public DbSet<PermissionDefinitionTranslation> PermissionDefinitionTranslations { get; set; } = null!;

    public DbSet<Invitation> Invitations { get; set; } = null!;
    public DbSet<TokenBlacklist> TokenBlacklists { get; set; } = null!;

    // Outbox and Inbox - required by IOutboxDbContext and IInboxDbContext
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;
    public DbSet<InboxRetryHistory> InboxRetryHistories { get; set; } = null!;

    // Identity Claims & Logins
    //
    // AuthDbContext can't inherit SmartEcommerce.BuildingBlock.Persistence.Ef.DbContext.DbContextBase - it must
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
