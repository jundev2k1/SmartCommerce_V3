using Elastic.Clients.Elasticsearch.Mapping;

namespace BuildingBlock.Search.Abstractions;

/// <summary>
/// The one reusable component allowed to write to Elasticsearch. Generic over the document
/// type so any future service's read-model document can reuse it without new BuildingBlock
/// code - query-side access is intentionally a separate, per-service concern (see the task's
/// Search Repository requirement), never mixed into this interface.
/// </summary>
public interface IElasticsearchIndexer<TDocument> where TDocument : class
{
    /// <summary>Creates the index with the given mapping only if it doesn't already exist. Safe to call on every service startup.</summary>
    Task EnsureIndexAsync(string indexName, Action<PropertiesDescriptor<TDocument>> configureMapping, CancellationToken ct = default);

    /// <summary>Drops and recreates the index with the given mapping - used by rebuild flows, never by the live sync path.</summary>
    Task RecreateIndexAsync(string indexName, Action<PropertiesDescriptor<TDocument>> configureMapping, CancellationToken ct = default);

    Task IndexAsync(string indexName, string documentId, TDocument document, CancellationToken ct = default);

    Task DeleteAsync(string indexName, string documentId, CancellationToken ct = default);

    Task BulkIndexAsync(string indexName, IEnumerable<(string Id, TDocument Document)> documents, CancellationToken ct = default);
}
