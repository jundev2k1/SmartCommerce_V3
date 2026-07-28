using Elastic.Clients.Elasticsearch.IndexManagement;
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

    /// <summary>
    /// Overload accepting index settings (e.g. a custom analyzer) alongside the mapping - the escape
    /// hatch for services that need more than field-level mapping (e.g. accent-insensitive search).
    /// Additive: does not change behavior for existing callers of the 3-arg overload above.
    /// </summary>
    Task EnsureIndexAsync(
        string indexName,
        Action<PropertiesDescriptor<TDocument>> configureMapping,
        Action<IndexSettingsDescriptor<TDocument>> configureSettings,
        CancellationToken ct = default);

    /// <summary>Drops and recreates the index with the given mapping - used by rebuild flows, never by the live sync path.</summary>
    Task RecreateIndexAsync(string indexName, Action<PropertiesDescriptor<TDocument>> configureMapping, CancellationToken ct = default);

    /// <summary>See <see cref="EnsureIndexAsync(string, Action{PropertiesDescriptor{TDocument}}, Action{IndexSettingsDescriptor{TDocument}}, CancellationToken)"/>.</summary>
    Task RecreateIndexAsync(
        string indexName,
        Action<PropertiesDescriptor<TDocument>> configureMapping,
        Action<IndexSettingsDescriptor<TDocument>> configureSettings,
        CancellationToken ct = default);

    Task IndexAsync(string indexName, string documentId, TDocument document, CancellationToken ct = default);

    Task DeleteAsync(string indexName, string documentId, CancellationToken ct = default);

    Task BulkIndexAsync(string indexName, IEnumerable<(string Id, TDocument Document)> documents, CancellationToken ct = default);
}
