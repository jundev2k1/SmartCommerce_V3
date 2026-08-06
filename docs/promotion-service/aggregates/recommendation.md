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
| `RecommendationProgramTranslation` | `BaseEntity<Guid>` | Added Phase 2.5: `Id = RecommendationProgram.Id`, composite `(Id, LanguageCode)`. Exposed via `RecommendationProgram.Translate(languageCode, name, description)` — upsert, per [../entities/translation-workflow.md](../entities/translation-workflow.md) |

## Enums

- `ProgramStatus` (shared, consolidated Phase 2.5) — Draft/Active/Paused/Expired/Archived. Was `RecommendationProgramStatus` until merged with 3 identical siblings — see [../enums/README.md](../enums/README.md).
- `RecommendationType` — CrossSell/UpSell/FrequentlyBoughtTogether/Trending/Manual/AI (given explicitly).

## Value Objects

- `EntityCode` (shared, consolidated Phase 2.5) — used by `RecommendationProgram.Code`.
- `Period` (shared, consolidated Phase 2.5, reserved) — `RecommendationProgram` has no `TimeZone` property.

## Indexes (design only — written in Phase 3)

`(Code)` unique · `(Status)` · `(Type)` · `RecommendationProduct: (ProductId)`

## Reconciliation notes

No `EntityData`/`UpdateData`/`RecommendationProgramTranslationData` wrapper types created (Domain Rule 2).
