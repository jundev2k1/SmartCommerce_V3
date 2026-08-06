# Promotion Service — Value Objects

**Scope:** Catalogue of every `Promotion.Domain` Value Object, updated as each is introduced (step 7 of [../entities/entity-implementation-strategy.md](../entities/entity-implementation-strategy.md)). All are PromotionService-local — none of the shared `BuildingBlock.Domain.ValueObjects` types fit a Code/Period/Priority shape, so none were reused-in-place here (the shared `LanguageCode` *is* reused directly for `CampaignLocalization`, no local wrapper).

| Value Object | Base | Shape | Used by |
|---|---|---|---|
| `CampaignCode` | `StringValueObject` | Uppercase alphanumeric + `_`/`-`, max 50 chars | `Campaign.Code` |
| `CampaignPeriod` | `ValueObject` | `StartTime`/`EndTime`/`TimeZone`, `EndTime > StartTime` enforced | `CampaignSchedule.Period` |
| `PromotionCode` | `StringValueObject` | Same shape as `CampaignCode` | `Promotion.Code` |
| `PromotionPeriod` | `ValueObject` | Same shape as `CampaignPeriod` | reserved — not yet consumed by a Phase 2.2 entity (Promotion keeps plain scalar Start/End/TimeZone per its literal Properties list); available for a future per-entry period need |
| `PromotionPriorityValue` | `ValueObject` | Validated `int` 0-100 (mirrors `BuildingBlock.Domain.ValueObjects.Percentage`) | `PromotionPriority.Value` |

`Campaign`/`Promotion` themselves keep `StartTime`/`EndTime`/`TimeZone` as plain scalar properties, not wrapped in `CampaignPeriod`/`PromotionPeriod` — the phase brief's Properties list specified them as three separate flat fields, so the VO is used where a *reusable* period shape is actually needed (`CampaignSchedule`) rather than forced onto the root's own literal field list.
