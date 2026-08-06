# Recommendation Aggregate

**Scope:** Domain-layer facts for the `RecommendationProgram` aggregate, implemented Phase 2.4. Structural only — no business rule (scoring, ranking, ML inference) is implemented yet.

## Aggregate boundary

`RecommendationProgram` (`Promotion.Domain/Entities/Recommendations/RecommendationProgram.cs`) is the aggregate root — `AggregateRoot<Guid>`, `IAuditable`, `ITenantEntity`. It owns, via navigation + internal construction: `RecommendationRule`, `RecommendationProduct`.

`RecommendationScore` and `RecommendationHistory` are **not** part of the navigation graph (no Navigation section given for either) — both get a public `Create`. `RecommendationScore` is notably not even `ProgramId`-scoped — it's a standalone, `ProductId`-keyed signal, independent of any one recommendation program.

## Entities

| Entity | Shape | Notes |
|---|---|---|
| `RecommendationProgram` | `AggregateRoot<Guid>` | Code/Name/Description/Status/RecommendationType/StartTime/EndTime/Priority — no `TimeZone` property, same as `LoyaltyProgram` |
| `RecommendationRule` | `BaseEntity<Guid>` | ProgramId/RuleType/Configuration (opaque string blob)/Priority |
| `RecommendationProduct` | `BaseEntity<Guid>` | ProgramId/ProductId/Score (plain decimal)/DisplayOrder |
| `RecommendationScore` | `BaseEntity<Guid>` | ProductId/ScoreType/ScoreValue/CalculatedAt — public `Create`, not `ProgramId`-scoped |
| `RecommendationHistory` | `BaseEntity<Guid>` | ProgramId/Action/OperatorId — `CreatedAt` inherited, public `Create` |

## Enums

- `RecommendationProgramStatus` — Draft/Active/Paused/Expired/Archived (given explicitly).
- `RecommendationType` — CrossSell/UpSell/FrequentlyBoughtTogether/Trending/Manual/AI (given explicitly).

## Value Objects

- `RecommendationCode` — same shape as `CampaignCode`.
- `RecommendationPeriod` — same shape as `CampaignPeriod` minus `TimeZone` (matches `RecommendationProgram` having no `TimeZone` property) — reserved, not yet consumed.

## Indexes (design only — written in Phase 3)

`(Code)` unique · `(Status)` · `(Type)` · `RecommendationProduct: (ProductId)`

## Reconciliation notes

No `EntityData`/`UpdateData` wrapper types created (Domain Rule 2).
