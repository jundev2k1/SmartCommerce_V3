using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using NovaCore.BuildingBlock.Persistence.Ef.DbContext;

namespace NovaCore.Auth.Persistence.Engine;

/// <summary>
/// Design-time factory used only by `dotnet ef migrations` tooling. Building AuthDbContext
/// through the full application host fails at design time - the Identity 10.0
/// AddEntityFrameworkStores&lt;AuthDbContext&gt; registration resolves a UserStore generic
/// signature the host's DI container can't satisfy for design-time activation - so migrations
/// are generated against a bare DbContextOptions instead. Mirrors AddPersistenceDbContext's
/// options exactly (snake_case naming convention included) so the scaffolded migration matches
/// the runtime model. Runtime resolution (Auth.API via AddPersistence) is unaffected.
/// </summary>
public sealed class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=auth_db;User Id=postgres;Password=postgres;");
        optionsBuilder.UseSnakeCaseNamingConvention();

        return new AuthDbContext(optionsBuilder.Options);
    }
}
