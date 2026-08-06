namespace NovaCore.Promotion.Domain.Entities.Approvals;

/// <summary>A single ordered step in an ApprovalWorkflow - Status is a plain string, distinct from the workflow-level ApprovalWorkflowStatus (no dedicated step-status enum requested).</summary>
public sealed class ApprovalStep : BaseEntity<Guid>, IAuditable
{
    public Guid WorkflowId { get; private set; }
    public int StepOrder { get; private set; }
    public string ApproverRole { get; private set; } = string.Empty;
    public string? Status { get; private set; }

    private ApprovalStep() { }

    /// <summary>Only ApprovalWorkflow may construct an ApprovalStep - see ApprovalWorkflow.AddStep.</summary>
    internal static ApprovalStep Create(Guid workflowId, int stepOrder, string approverRole)
    {
        if (string.IsNullOrWhiteSpace(approverRole))
            throw ExceptionFactory.RequiredField("Approver role cannot be empty.");

        return new ApprovalStep
        {
            Id = Guid.CreateVersion7(),
            WorkflowId = workflowId,
            StepOrder = stepOrder,
            ApproverRole = approverRole,
        };
    }

    public void UpdateStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw ExceptionFactory.RequiredField("Step status cannot be empty.");

        Status = status;
    }
}
