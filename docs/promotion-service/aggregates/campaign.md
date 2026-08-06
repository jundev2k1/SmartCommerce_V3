# Campaign Aggregate

**Scope:** Domain-layer facts for the `Campaign` aggregate, implemented Phase 2.1. Structural only — no business rule (eligibility, budget calculation, approval workflow) is implemented yet.

## Aggregate boundary

`Campaign` (`Promotion.Domain/Entities/Campaigns/Campaign.cs`) is the aggregate root — `AggregateRoot<Guid>`, `IAuditable`, `ITenantEntity`. It owns, via `ICollection<T>` navigation + internal-construction (Rule 3): `CampaignSchedule`, `CampaignAudience`, `CampaignChannel`, `CampaignTag`, `CampaignAttachment`, `CampaignLocalization`. `Promotions` is an **inverse-only** navigation — `Promotion` is its own aggregate root (`Promotion.CampaignId` is the FK), Campaign never constructs or removes one.

`CampaignBudget` and `CampaignApproval` are **not** part of this aggregate's navigation graph — both exist as their own entities in the same `Entities/Campaigns/` folder, related only by `CampaignId` (and `Campaign.BudgetId` for the budget). This follows the phase brief's own Navigation list, which omitted both. `CampaignBudget` keeps its own surrogate `Id` (budget history justification, same reasoning `UserRoleAssignment` already documents). `CampaignApproval` is a minimal structural placeholder — no approval-status enum/workflow yet, deferred to **Phase 2.5 (Approval + Validation + Audit)** per the "never anticipate future prompts" rule (see [../planning/PROGRESS.md](../planning/PROGRESS.md) for the authoritative Phase 2.x sequence).

## Entities

| Entity | Shape | Notes |
|---|---|---|
| `Campaign` | `AggregateRoot<Guid>` | Code/Name/Description/Status/Type/Priority/BudgetId/StartTime/EndTime/TimeZone/DisplayOrder/IsEnabled |
| `CampaignSchedule` | `BaseEntity<Guid>` | `CampaignPeriod` VO + optional label |
| `CampaignAudience` | `BaseEntity<Guid>` | Named targeting segment, no eligibility evaluation |
| `CampaignChannel` | `BaseEntity<Guid>` | Plain string channel code (no `ChannelType` enum requested) |
| `CampaignTag` | `BaseEntity<Guid>` | Freeform label owned directly by Campaign — no separate definition catalog (unlike Order's `OrderTagDefinition`/`OrderTag` split) |
| `CampaignAttachment` | `BaseEntity<Guid>` | Opaque `Url` — this service does not own file storage |
| `CampaignLocalization` | `BaseEntity<Guid>` | Translation pattern (Rule 5): `Id = Campaign.Id`, composite `(Id, LanguageCode)` |
| `CampaignBudget` | `BaseEntity<Guid>` | `AllocatedAmount`/`SpentAmount` (`Money`), `RecordSpend` is a structural setter, not an accumulator |
| `CampaignApproval` | `BaseEntity<Guid>` | Placeholder request/decision fields only — real workflow is a later Phase 2.x (Approval + Validation + Audit) |

## Enums

- `CampaignStatus` — Draft/Scheduled/Active/Paused/Completed/Cancelled.
- `CampaignType` — structural placeholder taxonomy (Seasonal/FlashSale/Loyalty/Acquisition/Retention/Clearance/Custom); not architect-specified, confirm before treating as final.

## Value Objects

- `CampaignCode` — uppercase alphanumeric + `_`/`-`, max 50 chars (`StringValueObject`, mirrors `Product.Domain.ValueObjects.Slug`'s shape).
- `CampaignPeriod` — `StartTime`/`EndTime`/`TimeZone` triple, `EndTime > StartTime` enforced (`ValueObject`, mirrors `Payment.Domain.ValueObjects.BillingAddress`'s shape). Used by `CampaignSchedule`; `Campaign` itself keeps `StartTime`/`EndTime`/`TimeZone` as plain scalars per the phase brief's literal Properties list.

## Indexes (design only — written in Phase 3)

- `(Code)` unique
- `(Status, StartTime)`
- `(Type, Status)`
- `(TenantId, Code)` unique

## Reconciliation notes

- **No `CreateCampaignData` (EntityData)** — no child collection is structurally mandatory at creation (no stated "≥1 X" rule), so `Campaign.Create` takes flat scalar parameters (Rule 2) and every child is added via a separate `Add{Child}` method afterward, same as `ProductBrand`.
- **No `UpdateData` wrapper** — `UpdateDetails`/`Reschedule`/etc. all take flat parameters, matching `ProductBrand.UpdateDetails`.
