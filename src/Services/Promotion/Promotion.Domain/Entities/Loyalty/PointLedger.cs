namespace NovaCore.Promotion.Domain.Entities.Loyalty;

/// <summary>A double-entry ledger row for a PointTransaction - not navigated from PointTransaction (no Navigation section given for it), so construction is public. No balance calculation lives here.</summary>
public sealed class PointLedger : BaseEntity<Guid>, IAuditable
{
    public Guid TransactionId { get; private set; }
    public int Debit { get; private set; }
    public int Credit { get; private set; }
    public int Balance { get; private set; }

    private PointLedger() { }

    public static PointLedger Create(Guid transactionId, int debit, int credit, int balance)
    {
        return new PointLedger
        {
            Id = Guid.CreateVersion7(),
            TransactionId = transactionId,
            Debit = debit,
            Credit = credit,
            Balance = balance,
        };
    }
}
