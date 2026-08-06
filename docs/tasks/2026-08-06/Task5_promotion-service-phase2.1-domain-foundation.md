# Task 5: Promotion Service — Phase 2.1 Domain Foundation

**Status:** Done (structural + documentation only, per the prompt's own scope)
**Category:** Domain-layer scaffolding + convention documentation, no business entities

## What was done

Prepared the Domain-layer folder structure and confirmed/wrote down the per-aggregate implementation order that every future Promotion aggregate must follow — no aggregate itself was implemented (Campaign is explicitly the next prompt, 2.2).

**Folder structure** — added to `Promotion.Domain/` (all empty, `.gitkeep`-marked): `Events/`, `Metadata/`, `Constants/`. `Entities/`, `Enums/`, `ValueObjects/` already existed from Phase 1.

Before creating anything, audited the real Domain-layer folder conventions across `Auth.Domain`/`User.Domain`/`Order.Domain`/`Product.Domain` (full file listing per project) rather than trusting the phase brief's example folder list at face value. Findings:
- **Real and reused**: `Entities/{Group}/{Entity}.cs` (+ sibling `{Entity}Translation.cs`, + `Entities/{Group}/Data/Create{Entity}Data.cs` for bulk-create aggregates), `Enums/`, `ValueObjects/`, `Events/` (User.Domain), `Metadata/` (Auth/User/Order/Product), `Constants/` (Auth.Domain: `SessionDefaults`/`MfaDefaults`/`InvitationDefaults`).
- **Not real, not created**: `Abstractions/` (an Application-layer folder, confirmed via `*.Application/Abstractions/`), `Aggregates/` (no such folder anywhere — the aggregate root is just the entity at the top of `Entities/{Group}/`), `Specifications/` (`docs/conventions/domain-coding-conventions.md` Rules 1/2 explicitly reject Spec-wrapper objects; `Product.Domain/Entities/Specifications/` is an unrelated business entity group, not the DDD pattern), `Builders/` (no precedent in any `*.Domain` project).
- **Not real, terminology reconciled instead of created**: `UpdateData`/`TranslationData` — no such wrapper record exists anywhere; every update/translate method in the real codebase (`ProductBrand.UpdateDetails`/`ProductBrand.Translate`) takes flat parameters, which is Rule 2 ("No Spec/Request/Command/DTO-like objects inside Domain"). Folded into "Structural methods" and "Translation" respectively in the rewritten strategy doc instead of being created as new types.

**Documentation** — rewrote [docs/promotion-service/entities/entity-implementation-strategy.md](../../promotion-service/entities/entity-implementation-strategy.md) from the Phase-0-era generic 11-step order into a grounded 12-step order (Entity → EntityData → Properties → Navigation Properties → Translation → Enums → Value Objects → Owned Types → Indexes & Unique Constraints → Relationships → Structural methods → Aggregate Boundaries & Future TODO), with a "Reconciliation" section explaining every divergence from the brief's original wording and citing the real reference file for each pattern (`Order.Domain/Entities/Orders/Data/CreateOrderData.cs`, `Product.Domain/Entities/Brands/ProductBrand.cs`). Updated `Promotion.Domain/TODO.md` to match.

## Objective

Establish a stable, grounded Domain-layer foundation so every future aggregate prompt (starting with Campaign, 2.2) only has to focus on entity implementation — no further convention research needed mid-aggregate.

## Current state (grounded findings)

- `Promotion.Domain` still has zero entities/enums/Value Objects — this prompt added only empty structural folders and documentation.
- The platform-wide `docs/conventions/domain-coding-conventions.md` is the actual binding style authority; this task's strategy doc defers to it rather than restating it, and only adds the *ordering* + Promotion-specific reconciliation notes.

## Scope

**Built this prompt:** `Events/`/`Metadata`/`Constants/` folder placeholders, rewritten entity-implementation-strategy.md, updated Domain TODO.md.

**Explicitly not built:** any Campaign (or other) entity, enum, Value Object — Phase 2.2's job.

## Dependencies

Phase 1 (Bootstrap Service). Phase 2.2 (Campaign Aggregate) depends on this foundation.

## Estimated complexity

Small (documentation + empty folders, no business code, no build).

## Risks

None expected — this prompt is pure convention-alignment work with no runtime surface. If the architect's design uses domain-event/metadata/constants patterns this audit didn't anticipate, reconcile when the first aggregate that needs one is implemented, same as every other "populated only when needed" folder in this tree.
