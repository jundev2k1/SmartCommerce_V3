# Promotion Service — Entity Implementation Strategy

**Scope:** The binding, fixed order every aggregate follows during Phase 2 (Domain Model). This is a process rule, not a per-entity spec — no entity exists yet, so nothing below names an actual Promotion type. Per-entity docs land under [../aggregates/](../aggregates/) as each one is implemented in that order.

This strategy augments, not replaces, [../../conventions/domain-coding-conventions.md](../../conventions/domain-coding-conventions.md) — general Domain-layer style rules (aggregate creation shape, no Spec objects, plain navigation collections, many-to-many via mapping entities, reusable Value Object validation) still apply on top of the ordering below.

## Fixed order, per aggregate

1. **Entity** — declare the aggregate/entity type and its identity (`Id`), nothing else yet.
2. **Properties** — scalar properties exactly as the architect's design specifies. No property invented to "seem complete."
3. **Navigation Properties** — references to other entities/aggregates, per the design's stated relationships only.
4. **Enums** — any enum the entity's properties require, added under [../enums/](../enums/) as it's introduced.
5. **Value Objects** — any Value Object the entity's properties require, added under [../value-objects/](../value-objects/) as it's introduced. Reuse `BuildingBlock.Domain.ValueObjects` where the shared type fits; only add a PromotionService-local Value Object when the shared one doesn't (same precedent Payment Service's local `Money`/`Currency` set — see [../../services/payment-service.md](../../services/payment-service.md#value-objects)).
6. **Owned Types** — EF-owned-type mappings for Value Objects with no identity of their own. This step is *design* only in Phase 2 (the actual `OwnsOne` mapping is written in Phase 3) — Phase 2 just confirms which properties are owned-type candidates.
7. **Indexes** — identify which properties need a query-supporting index. Design only in Phase 2; written in Phase 3, catalogued under [../indexes/](../indexes/).
8. **Unique Constraints** — identify which properties/property-combinations must be unique. Design only in Phase 2; written in Phase 3.
9. **Relationships** — confirm FK direction and cardinality for every Navigation Property from step 3 (one-to-many, one-to-one, many-to-many via mapping entity — never a raw EF many-to-many).
10. **Aggregate Boundaries** — confirm which entities are the Aggregate Root and which are children only reachable through it; document the boundary under [../aggregates/](../aggregates/).
11. **Future TODO** — anything the architect's design references but explicitly defers (a field, a relationship, a behavior) gets a `// TODO:` comment in code and a one-line note in the aggregate's doc — never a guessed implementation.

## Rules

- No step is skipped. No step is reordered. No step for aggregate B starts before aggregate A's steps 1-11 are done, unless the design explicitly says two aggregates are implemented together (e.g. an aggregate root and a child entity with no independent lifecycle).
- No entity is created beyond what the architect's design specifies — this phase implements a design, it does not extend one.
