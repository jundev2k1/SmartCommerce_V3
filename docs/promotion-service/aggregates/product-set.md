# Product Set Aggregate

**Scope:** Domain-layer facts for the `ProductSet` aggregate, implemented Phase 2.4. Structural only — no business rule (bundle pricing calculation, availability) is implemented yet.

## Aggregate boundary

`ProductSet` (`Promotion.Domain/Entities/ProductSets/ProductSet.cs`) is the aggregate root — `AggregateRoot<Guid>`, `IAuditable`, `ITenantEntity`. It owns, via navigation + internal construction: `ProductSetItem`, `ProductBundle`.

`BundlePrice`, `BundleRule`, `BundleGift` are **not** part of the navigation graph — no Navigation section was given for `ProductBundle` itself, so its own children are related by `BundleId` only, with a public `Create`, exactly one level removed from the aggregate root (same shape as `PromotionCondition` belonging to `PromotionRule` in Phase 2.1).

## `Quantity` reused for every item/gift count

`ProductSetItem.Quantity` and `BundleGift.Quantity` both use the shared `BuildingBlock.Domain.ValueObjects.Quantity` — the same Value Object `Order.Domain`'s `OrderItem` already uses for its own `Quantity`. No PromotionService-local Quantity VO was created.

## Entities

| Entity | Shape | Notes |
|---|---|---|
| `ProductSet` | `AggregateRoot<Guid>` | Code/Name/Description/Status/SetType |
| `ProductSetItem` | `BaseEntity<Guid>` | ProductSetId/ProductId/VariantId/Quantity (shared VO) |
| `ProductBundle` | `BaseEntity<Guid>` | ProductSetId/Name/DisplayOrder |
| `BundlePrice` | `BaseEntity<Guid>` | BundleId/Currency/Price (shared `Money` VO) — public `Create` |
| `BundleRule` | `BaseEntity<Guid>` | BundleId/RuleType/Configuration (opaque string blob) — public `Create` |
| `BundleGift` | `BaseEntity<Guid>` | BundleId/ProductId/Quantity (shared VO) — public `Create` |

## Enums

- `ProductSetStatus` — Draft/Active/Archived (given explicitly).
- `ProductSetType` — Bundle/Combo/Kit/Collection (given explicitly).

## Value Objects

- `ProductSetCode` — same shape as `CampaignCode`.
- `Money` (shared, `BuildingBlock.Domain.ValueObjects`) — used by `BundlePrice.Price`, paired with a separate `Currency` scalar, same split Voucher/CampaignBudget already use.
- `Quantity` (shared, `BuildingBlock.Domain.ValueObjects`) — used by `ProductSetItem.Quantity`/`BundleGift.Quantity`.

## Indexes (design only — written in Phase 3)

`(Code)` unique · `(Status)`

## Reconciliation notes

No `EntityData`/`UpdateData` wrapper types created (Domain Rule 2).
