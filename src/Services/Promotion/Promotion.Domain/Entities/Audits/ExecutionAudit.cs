namespace NovaCore.Promotion.Domain.Entities.Audits;

/// <summary>An audit row scoped to an execution-shaped entity (RewardExecution, DistributionExecution, ...) - CreatedAt is inherited from BaseEntity, not redeclared.</summary>
public sealed class ExecutionAudit : BaseEntity<Guid>, IAuditable
{
    public Guid ExecutionId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public Guid? OperatorId { get; private set; }

    private ExecutionAudit() { }

    public static ExecutionAudit Create(Guid executionId, string action, Guid? operatorId = null)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw ExceptionFactory.RequiredField("Audit action cannot be empty.");

        return new ExecutionAudit
        {
            Id = Guid.CreateVersion7(),
            ExecutionId = executionId,
            Action = action,
            OperatorId = operatorId,
        };
    }
}
