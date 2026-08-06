# Domain — TODO

**Phase:** 2 (Domain Model). Bootstrap phase (1) only creates this placeholder — no entity exists yet.

Implement every aggregate the architect's design specifies, following the fixed 11-step order in [../../../docs/promotion-service/entities/entity-implementation-strategy.md](../../../docs/promotion-service/entities/entity-implementation-strategy.md) (Entity → Properties → Navigation Properties → Enums → Value Objects → Owned Types → Indexes → Unique Constraints → Relationships → Aggregate Boundaries → Future TODO).

Target folders (create as each aggregate is implemented):
- `Entities/{Group}/{Aggregate}.cs`
- `Enums/{EnumName}.cs`
- `ValueObjects/{ValueObjectName}.cs`

No EF Core, no MediatR, no repository types belong in this project — see [../../../docs/promotion-service/phases/phase-2-domain-model.md](../../../docs/promotion-service/phases/phase-2-domain-model.md).
