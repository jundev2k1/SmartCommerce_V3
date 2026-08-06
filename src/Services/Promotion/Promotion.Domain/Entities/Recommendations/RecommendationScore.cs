namespace NovaCore.Promotion.Domain.Entities.Recommendations;

/// <summary>A standalone, product-keyed scoring signal (not tied to one specific RecommendationProgram) - not navigated from RecommendationProgram, so construction is public. No score calculation lives here.</summary>
public sealed class RecommendationScore : BaseEntity<Guid>, IAuditable
{
    public Guid ProductId { get; private set; }
    public string ScoreType { get; private set; } = string.Empty;
    public decimal ScoreValue { get; private set; }
    public DateTime CalculatedAt { get; private set; }

    private RecommendationScore() { }

    public static RecommendationScore Create(Guid productId, string scoreType, decimal scoreValue)
    {
        if (string.IsNullOrWhiteSpace(scoreType))
            throw ExceptionFactory.RequiredField("Score type cannot be empty.");

        return new RecommendationScore
        {
            Id = Guid.CreateVersion7(),
            ProductId = productId,
            ScoreType = scoreType,
            ScoreValue = scoreValue,
            CalculatedAt = DateTime.UtcNow,
        };
    }
}
