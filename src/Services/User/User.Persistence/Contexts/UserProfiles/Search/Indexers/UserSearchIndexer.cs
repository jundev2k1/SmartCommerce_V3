using BuildingBlock.Search.Abstractions;

using User.Application.Abstractions.Search;
using User.Persistence.Contexts.UserProfiles.Search.Mapping;

namespace User.Persistence.Contexts.UserProfiles.Search.Indexers;

/// <summary>
/// IUserSearchIndexer impl - fixes the User index name/mapping/settings on top of
/// BuildingBlock.Search's generic, reusable IElasticsearchIndexer&lt;&gt;.
/// </summary>
public sealed class UserSearchIndexer(IElasticsearchIndexer<UserSearchDocument> indexer) : IUserSearchIndexer
{
    public Task EnsureIndexAsync(CancellationToken ct = default) =>
        indexer.EnsureIndexAsync(UserSearchIndexNames.Default, UserSearchIndexMapping.Configure, UserSearchIndexMapping.ConfigureSettings, ct);

    public Task RecreateIndexAsync(CancellationToken ct = default) =>
        indexer.RecreateIndexAsync(UserSearchIndexNames.Default, UserSearchIndexMapping.Configure, UserSearchIndexMapping.ConfigureSettings, ct);

    public Task IndexAsync(UserSearchDocument document, CancellationToken ct = default) =>
        indexer.IndexAsync(UserSearchIndexNames.Default, document.UserId.ToString(), document, ct);

    public Task DeleteAsync(Guid userId, CancellationToken ct = default) =>
        indexer.DeleteAsync(UserSearchIndexNames.Default, userId.ToString(), ct);

    public Task BulkIndexAsync(IEnumerable<UserSearchDocument> documents, CancellationToken ct = default) =>
        indexer.BulkIndexAsync(
            UserSearchIndexNames.Default,
            documents.Select(d => (d.UserId.ToString(), d)),
            ct);
}
