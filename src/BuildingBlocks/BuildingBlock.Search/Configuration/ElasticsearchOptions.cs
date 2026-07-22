namespace BuildingBlock.Search.Configuration;

public sealed class ElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";

    public string Url { get; set; } = string.Empty;

    public int MaxRetries { get; set; } = 3;

    public int RequestTimeoutSeconds { get; set; } = 30;
}
