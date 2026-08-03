namespace SmartEcommerce.User.Persistence.Contexts.UserProfiles.Search;

/// <summary>
/// The only place the literal User Search index name lives. Since <see cref="SmartEcommerce.BuildingBlock.Search.Indexing.ElasticsearchIndexer{TDocument}"/>'s
/// alias-based blue/green reindexing (2026-07-28, Task 20), this is an ES alias, not a concrete
/// index - the real index behind it is versioned and swapped atomically on every rebuild.
/// </summary>
public static class UserSearchIndexNames
{
    public const string Default = "user-search";
}
