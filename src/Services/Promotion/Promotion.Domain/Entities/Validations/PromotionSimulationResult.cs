namespace NovaCore.Promotion.Domain.Entities.Validations;

/// <summary>The outcome of running a PromotionSimulationScenario - Output is an opaque string blob, no simulation execution logic lives here.</summary>
public sealed class PromotionSimulationResult : BaseEntity<Guid>, IAuditable
{
    public Guid ScenarioId { get; private set; }
    public string? Output { get; private set; }
    public string Status { get; private set; } = string.Empty;

    private PromotionSimulationResult() { }

    public static PromotionSimulationResult Create(Guid scenarioId, string status, string? output = null)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw ExceptionFactory.RequiredField("Result status cannot be empty.");

        return new PromotionSimulationResult
        {
            Id = Guid.CreateVersion7(),
            ScenarioId = scenarioId,
            Status = status,
            Output = output,
        };
    }
}
