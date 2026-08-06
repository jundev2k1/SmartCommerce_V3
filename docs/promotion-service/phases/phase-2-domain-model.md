# Phase 2 — Domain Model

## Purpose

Implement the Promotion domain model exactly as already designed by the system architect — aggregates, entities, enums, Value Objects, owned types, navigation properties, aggregate boundaries — following the fixed 11-step order in [../entities/entity-implementation-strategy.md](../entities/entity-implementation-strategy.md), one aggregate at a time. No persistence, no CQRS, no business logic beyond what the design specifies.

## Expected Output

- `Promotion.Domain/Entities/**`, `Promotion.Domain/Enums/**`, `Promotion.Domain/ValueObjects/**` populated per the architect's design.
- One doc per aggregate under [../aggregates/](../aggregates/) as it's implemented.
- One doc per Value Object under [../value-objects/](../value-objects/) as it's implemented.
- Enum catalogue under [../enums/](../enums/) kept current as enums are added.

## Build Verification

`Promotion.Domain` builds standalone, referencing only `BuildingBlock.Domain` — no `Promotion.Persistence`/`Promotion.Application` reference leaks in.

## Completion Criteria

- Every aggregate the architect's design specifies exists as a compilable Domain type with correct invariants/navigation.
- No EF Core, no MediatR, no repository types appear anywhere in `Promotion.Domain`.
- [../conventions/domain-coding-conventions.md](../../conventions/domain-coding-conventions.md) style rules are followed (aggregate creation shape, no Spec objects, plain navigation collections, many-to-many via mapping entities, reusable Value Object validation) — this phase does not introduce a new Domain-layer pattern.

## Blocked Items

- Persistence mapping is Phase 3's concern — no `IEntityTypeConfiguration<T>` written here.
- CQRS/API surface is Phase 5's concern — no handlers/endpoints written here.

## Dependencies

Phase 1 (Bootstrap Service).
