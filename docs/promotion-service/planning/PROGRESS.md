# Promotion Service — Progress

**Scope:** Living status tracker for the Promotion Service implementation roadmap. This is the permanent progress-tracking convention for the whole project — read at the start of every phase/prompt, updated only at the end (never mid-work). Never recreate the roadmap; never restart the plan; always continue from what's recorded here.

## Current Project Status

**Completed Phases**
- ✔ Phase 0 — Planning Freeze
- ✔ Phase 1 — Bootstrap Service

**Current Phase**
- ▶ Phase 2 — Domain Model

**Current Prompt**
- ✔ 2.1 — Domain Foundation + Campaign/Promotion aggregates (done)
- ▶ 2.2 — Coupon + Voucher (next, not yet started)
- 2.3 — Loyalty + Reward + Distribution
- 2.4 — Recommendation + Product Set + Gift
- 2.5 — Approval + Validation + Audit
- 2.6 — Domain Review + single Domain build

**Overall Progress**
2 / 7 Completed

**Status**
In Progress

**Remaining Phases**
- □ Phase 3 — Persistence
- □ Phase 4 — Search Integration
- □ Phase 5 — CQRS Skeleton
- □ Phase 6 — Infrastructure Integration
- □ Phase 7 — Migration Preparation

## Progress Management Policy

- At the start of every phase/prompt: read this document. Never ask for progress clarification — always continue from what's recorded here.
- At the end of every phase: mark the completed phase, move the current-phase pointer, update the completed-phase count, update the status. Never recreate the roadmap or restart the plan.
- Within a phase, sub-prompts (e.g. Phase 2.1, 2.2, ...) are tracked under **Current Prompt** without moving the phase pointer or the Overall Progress count — the phase only counts as complete once every aggregate/prompt inside it is done.
- Sub-prompt numbering is authoritative from the most recently issued prompt — if a later prompt renumbers a sub-prompt, that renumbering wins and this file is updated to match (this happened once already: Domain Foundation + Campaign/Promotion were both folded into "2.1").
- **Commit granularity (added 2026-08-06):** every aggregate root implemented in a Domain-implementation prompt gets its own commit (entities + enums + Value Objects + aggregate doc). Cross-cutting structure and cross-cutting documentation each get their own commit, separate from any aggregate. See [../README.md](../README.md#implementation-mode-rules-binding-for-every-future-phase).

## History

- **2026-08-06** — Phase 0 (Planning Freeze) completed: roadmap ([roadmap.md](roadmap.md)), phase breakdown ([../phases/](../phases/)), documentation structure, Entity/CQRS/Persistence/Search strategy docs. See [../../tasks/2026-08-06/Task3_promotion-service-phase0-planning-freeze.md](../../tasks/2026-08-06/Task3_promotion-service-phase0-planning-freeze.md).
- **2026-08-06** — Phase 1 (Bootstrap Service) completed: cloned the 5-project Clean Architecture skeleton from Payment Service's foundation-phase precedent (`src/Services/Promotion/`), registered in `NovaCore.sln`, wired into `docker-compose.yml`/`docker-compose.override.yml`/`.env.template`/`scripts/postgres/init.sql`/the YARP gateway. `PromotionDbContext` carries Outbox/Inbox only — no aggregate exists yet. One verification build performed, 0 errors. See [../../tasks/2026-08-06/Task4_promotion-service-phase1-bootstrap-service.md](../../tasks/2026-08-06/Task4_promotion-service-phase1-bootstrap-service.md).
- **2026-08-06** — Phase 2.1 (Domain Foundation) completed: added `Events/`/`Metadata`/`Constants/` folders to `Promotion.Domain`; rewrote [../entities/entity-implementation-strategy.md](../entities/entity-implementation-strategy.md) into the grounded 12-step per-aggregate order. See [../../tasks/2026-08-06/Task5_promotion-service-phase2.1-domain-foundation.md](../../tasks/2026-08-06/Task5_promotion-service-phase2.1-domain-foundation.md).
- **2026-08-06** — Phase 2.1 (Campaign + Promotion aggregates, folded into the same sub-prompt number by the issuing prompt) completed: implemented both aggregates in full — 9 Campaign entities (`Campaign`, `CampaignSchedule`, `CampaignAudience`, `CampaignChannel`, `CampaignTag`, `CampaignAttachment`, `CampaignLocalization`, `CampaignBudget`, `CampaignApproval`), 13 Promotion entities + `PromotionMetadata` (reclassified as a Value Object, not an entity — see [../aggregates/promotion.md](../aggregates/promotion.md)), 7 enums, 5 Value Objects. Structural only, no business rule. No build performed, per the phase's own build policy. See [../../tasks/2026-08-06/Task6_promotion-service-phase2.1-campaign-promotion-aggregates.md](../../tasks/2026-08-06/Task6_promotion-service-phase2.1-campaign-promotion-aggregates.md). **Remaining in Phase 2 — next prompt (2.2) implements Coupon + Voucher.**
