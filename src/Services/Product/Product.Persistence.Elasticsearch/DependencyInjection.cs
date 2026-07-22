using BuildingBlock.Search.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Product.Application.Abstractions.Search;
using Product.Persistence.Elasticsearch.Search;

namespace Product.Persistence.Elasticsearch;

public static class DependencyInjection
{
    public static IServiceCollection AddElasticsearchPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddElasticsearchClient(configuration);
        services.AddScoped<IProductSearchIndexer, ProductSearchIndexer>();
        services.AddScoped<IProductSearchRepository, ProductSearchRepository>();

        return services;
    }
}
