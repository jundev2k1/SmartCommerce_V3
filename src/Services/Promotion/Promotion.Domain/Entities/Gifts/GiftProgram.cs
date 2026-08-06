namespace NovaCore.Promotion.Domain.Entities.Gifts;

/// <summary>
/// Aggregate root for a GiftProgram - owns Items. GiftInventory/GiftReservation/GiftClaim/
/// GiftUsage are related by id only, not navigated from here - see
/// docs/promotion-service/aggregates/gift.md. No ValueObjects section was given for this
/// aggregate, same as Reward/Distribution/Gift's siblings this phase - Code stays a plain string.
/// </summary>
public sealed class GiftProgram : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public GiftProgramStatus Status { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }

    public ICollection<GiftItem> Items { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private GiftProgram() { }

    public static GiftProgram Create(
        string code,
        string name,
        DateTime startTime,
        DateTime endTime,
        string? description = null)
    {
        ValidateCode(code);
        ValidateName(name);
        ValidatePeriod(startTime, endTime);

        return new GiftProgram
        {
            Id = Guid.CreateVersion7(),
            Code = code,
            Name = name,
            Description = description,
            Status = GiftProgramStatus.Draft,
            StartTime = startTime,
            EndTime = endTime,
        };
    }
    #endregion

    #region Details & lifecycle
    public void UpdateDetails(string name, string? description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    public void Reschedule(DateTime startTime, DateTime endTime)
    {
        ValidatePeriod(startTime, endTime);

        StartTime = startTime;
        EndTime = endTime;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Gift program name cannot be empty.");
    }

    private static void ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw ExceptionFactory.RequiredField("Gift program code cannot be empty.");
    }

    private static void ValidatePeriod(DateTime startTime, DateTime endTime)
    {
        if (endTime <= startTime)
            throw ExceptionFactory.InvalidRange("End time must be after start time.");
    }
    #endregion

    #region Status
    public void Activate()
    {
        if (Status is not (GiftProgramStatus.Draft or GiftProgramStatus.Paused))
            throw ExceptionFactory.InvalidStatus($"Cannot activate a gift program in {Status} status.");

        Status = GiftProgramStatus.Active;
    }

    public void Pause()
    {
        if (Status != GiftProgramStatus.Active)
            throw ExceptionFactory.InvalidStatus($"Cannot pause a gift program in {Status} status.");

        Status = GiftProgramStatus.Paused;
    }

    public void Expire()
    {
        if (Status is not (GiftProgramStatus.Active or GiftProgramStatus.Paused))
            throw ExceptionFactory.InvalidStatus($"Cannot expire a gift program in {Status} status.");

        Status = GiftProgramStatus.Expired;
    }

    public void Archive()
    {
        if (Status != GiftProgramStatus.Expired)
            throw ExceptionFactory.InvalidStatus($"Cannot archive a gift program in {Status} status.");

        Status = GiftProgramStatus.Archived;
    }
    #endregion

    #region Item
    public void AddItem(Guid productId, Quantity quantity, Guid? variantId = null)
    {
        Items.Add(GiftItem.Create(Id, productId, quantity, variantId));
    }

    public void RemoveItem(Guid itemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw ExceptionFactory.EntityNotFound<GiftItem>(itemId);

        Items.Remove(item);
    }
    #endregion
}
