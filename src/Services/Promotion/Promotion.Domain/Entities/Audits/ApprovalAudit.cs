namespace NovaCore.Promotion.Domain.Entities.Audits;

/// <summary>An audit row scoped to an ApprovalWorkflow - CreatedAt is inherited from BaseEntity, not redeclared. Distinct from ApprovalHistory (Entities/Approvals/), which is workflow-owned; this is the platform-level Audit-group counterpart.</summary>
public sealed class ApprovalAudit : BaseEntity<Guid>, IAuditable
{
    public Guid WorkflowId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public Guid? OperatorId { get; private set; }

    private ApprovalAudit() { }

    public static ApprovalAudit Create(Guid workflowId, string action, Guid? operatorId = null)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw ExceptionFactory.RequiredField("Audit action cannot be empty.");

        return new ApprovalAudit
        {
            Id = Guid.CreateVersion7(),
            WorkflowId = workflowId,
            Action = action,
            OperatorId = operatorId,
        };
    }
}
