namespace NovaCore.BuildingBlock.Domain.Abstractions;

/// <summary>
/// Reserved marker for entities that will participate in a future global soft-delete pipeline.
/// Not wired to anything yet - no soft-delete behavior exists in the persistence pipeline today.
/// Deliberately independent of IAuditable/audit tracking, which remains its own concern.
/// </summary>
public interface ISoftDeleteEntity
{
}
