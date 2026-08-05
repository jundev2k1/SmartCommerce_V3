using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Persistence.Tenancy;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.Inventory.Persistence.Engine;

/// <summary>
/// Design-time factory used only by `dotnet ef migrations` tooling. EF's design-time host
/// activation doesn't resolve the full application DI container, so OnModelCreating's
/// this.GetService&lt;ITenantConventionRegistry&gt;()/this.GetService&lt;ICurrentTenantService&gt;()
/// calls (the Tenant Convention - see docs/reference/tenant-convention.md) fail without an
/// explicit application service provider supplying them. Runtime resolution (Inventory.API via
/// AddPersistence) is unaffected.
/// </summary>
public sealed class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var applicationServices = new ServiceCollection()
            .AddSingleton<ITenantConventionRegistry, TenantConventionRegistry>()
            .AddSingleton<ICurrentTenantService, NullCurrentTenantService>()
            .BuildServiceProvider();

        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();
        optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=inventory_db;User Id=postgres;Password=postgres;");
        optionsBuilder.UseSnakeCaseNamingConvention();
        optionsBuilder.UseApplicationServiceProvider(applicationServices);

        return new InventoryDbContext(optionsBuilder.Options);
    }
}
