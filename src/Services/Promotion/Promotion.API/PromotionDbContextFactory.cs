using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.API;

/// <summary>
/// Lets `dotnet ef` build <see cref="PromotionDbContext"/> directly, without booting the full
/// app host (Kafka producers, Redis, APM, ...) that <c>Program.cs</c> wires up. Tooling-only -
/// never invoked at runtime.
/// </summary>
public sealed class PromotionDbContextFactory : IDesignTimeDbContextFactory<PromotionDbContext>
{
    public PromotionDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=localhost;Port=5432;Database=promotion_db;User Id=postgres;Password=NovaCore@Postgres2026;";

        var optionsBuilder = new DbContextOptionsBuilder<PromotionDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        optionsBuilder.UseSnakeCaseNamingConvention();

        return new PromotionDbContext(optionsBuilder.Options);
    }
}
