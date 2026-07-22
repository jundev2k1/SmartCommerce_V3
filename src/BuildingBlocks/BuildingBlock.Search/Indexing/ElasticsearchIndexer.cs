using BuildingBlock.Search.Abstractions;

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Transport.Products.Elasticsearch;

namespace BuildingBlock.Search.Indexing;

public sealed class ElasticsearchIndexer<TDocument>(ElasticsearchClient client) : IElasticsearchIndexer<TDocument>
    where TDocument : class
{
    public async Task EnsureIndexAsync(string indexName, Action<PropertiesDescriptor<TDocument>> configureMapping, CancellationToken ct = default)
    {
        var exists = await client.Indices.ExistsAsync(indexName, ct);
        if (exists.Exists)
            return;

        await CreateIndexAsync(indexName, configureMapping, ct);
    }

    public async Task RecreateIndexAsync(string indexName, Action<PropertiesDescriptor<TDocument>> configureMapping, CancellationToken ct = default)
    {
        var exists = await client.Indices.ExistsAsync(indexName, ct);
        if (exists.Exists)
            await client.Indices.DeleteAsync(indexName, ct);

        await CreateIndexAsync(indexName, configureMapping, ct);
    }

    public async Task IndexAsync(string indexName, string documentId, TDocument document, CancellationToken ct = default)
    {
        var response = await client.IndexAsync(document, indexName, documentId, ct);
        EnsureSuccess(response, $"index document '{documentId}' into '{indexName}'");
    }

    public async Task DeleteAsync(string indexName, string documentId, CancellationToken ct = default)
    {
        // A delete for a document that no longer exists is a valid outcome for a read-model
        // sync (e.g. a redelivered event) - not surfaced as a failure.
        await client.DeleteAsync(indexName, documentId, ct);
    }

    public async Task BulkIndexAsync(string indexName, IEnumerable<(string Id, TDocument Document)> documents, CancellationToken ct = default)
    {
        var items = documents.ToList();
        if (items.Count == 0)
            return;

        var response = await client.BulkAsync(b =>
        {
            b.Index(indexName);
            foreach (var item in items)
                b.Index(item.Document, op => op.Id(item.Id));
        }, ct);

        if (response.Errors)
        {
            throw new InvalidOperationException(
                $"Bulk index into '{indexName}' failed for {response.ItemsWithErrors.Count()} document(s): {response.DebugInformation}");
        }
    }

    private async Task CreateIndexAsync(string indexName, Action<PropertiesDescriptor<TDocument>> configureMapping, CancellationToken ct)
    {
        var response = await client.Indices.CreateAsync<TDocument>(
            indexName, c => c.Mappings(m => m.Properties(configureMapping)), ct);
        EnsureSuccess(response, $"create index '{indexName}'");
    }

    private static void EnsureSuccess(ElasticsearchResponse response, string action)
    {
        if (!response.IsValidResponse)
            throw new InvalidOperationException($"Elasticsearch operation failed to {action}: {response.DebugInformation}");
    }
}
