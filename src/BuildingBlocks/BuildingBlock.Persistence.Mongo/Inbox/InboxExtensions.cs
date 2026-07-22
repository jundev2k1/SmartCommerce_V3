using BuildingBlock.Persistence.Inbox;
using BuildingBlock.Persistence.Mongo.MongoContext;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Driver;

namespace BuildingBlock.Persistence.Mongo.Inbox;

public static class InboxExtensions
{
    /// <summary>
    /// Ensure the Inbox collection's indexes exist. Call once from the derived Mongo context's
    /// constructor - Mongo has no OnModelCreating equivalent to apply configuration declaratively.
    /// </summary>
    public static void EnsureInboxIndexes(this IMongoCollection<InboxDocument> collection)
    {
        collection.Indexes.CreateOne(new CreateIndexModel<InboxDocument>(
            Builders<InboxDocument>.IndexKeys.Ascending(x => x.MessageId).Ascending(x => x.ConsumerName),
            new CreateIndexOptions { Name = "idx_inbox_message_consumer_unique", Unique = true }));

        collection.Indexes.CreateOne(new CreateIndexModel<InboxDocument>(
            Builders<InboxDocument>.IndexKeys.Ascending(x => x.ProcessedAt),
            new CreateIndexOptions { Name = "idx_inbox_processed_at" }));

        // Covers the InboxRetryHostedService poll: WHERE Status = Retrying AND NextRetryAt <= now.
        collection.Indexes.CreateOne(new CreateIndexModel<InboxDocument>(
            Builders<InboxDocument>.IndexKeys.Ascending(x => x.Status).Ascending(x => x.NextRetryAt),
            new CreateIndexOptions { Name = "idx_inbox_status_next_retry_at" }));
    }

    /// <summary>
    /// Register the generic Mongo inbox store for the given Mongo context type.
    /// The context must implement IInboxMongoContext.
    /// </summary>
    public static IServiceCollection AddMongoInboxStore<TContext>(this IServiceCollection services)
        where TContext : MongoContextBase, IInboxMongoContext
    {
        services.AddScoped<IInboxStore, MongoInboxStore<TContext>>();
        return services;
    }
}
