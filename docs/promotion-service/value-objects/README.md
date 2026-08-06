# Promotion Service — Value Objects

**Scope:** Catalogue of every `Promotion.Domain` Value Object, updated as each is introduced (step 7 of [../entities/entity-implementation-strategy.md](../entities/entity-implementation-strategy.md)). Nearly all are PromotionService-local — none of the shared `BuildingBlock.Domain.ValueObjects` types fit a Code/Period/Priority shape, so those weren't reused-in-place; `LanguageCode` and `Money` **are** reused directly (see below), no local wrapper for either.

| Value Object | Base | Shape | Used by |
|---|---|---|---|
| `CampaignCode` | `StringValueObject` | Uppercase alphanumeric + `_`/`-`, max 50 chars | `Campaign.Code` |
| `CampaignPeriod` | `ValueObject` | `StartTime`/`EndTime`/`TimeZone`, `EndTime > StartTime` enforced | `CampaignSchedule.Period` |
| `PromotionCode` | `StringValueObject` | Same shape as `CampaignCode` | `Promotion.Code` |
| `PromotionPeriod` | `ValueObject` | Same shape as `CampaignPeriod` | reserved — not yet consumed |
| `PromotionPriorityValue` | `ValueObject` | Validated `int` 0-100 (mirrors `BuildingBlock.Domain.ValueObjects.Percentage`) | `PromotionPriority.Value` |
| `CouponCode` | `StringValueObject` | Same shape as `CampaignCode` | `Coupon.Code`, `CouponCode` entity's own `Code` field — **name collides with the `CouponCode` entity**, see [../aggregates/coupon.md](../aggregates/coupon.md) for the alias fix |
| `CouponPeriod` | `ValueObject` | Same shape as `CampaignPeriod` | reserved — not yet consumed |
| `CouponUsageLimit` | `ValueObject` | Bundled `MaxUsage`/`MaxUsagePerUser` pair | reserved — not yet consumed (`Coupon` keeps them as flat scalars per its literal Properties list) |
| `VoucherCode` | `StringValueObject` | Same shape as `CampaignCode` | `Voucher.Code` |
| `VoucherPeriod` | `ValueObject` | Same shape as `CampaignPeriod` | reserved — not yet consumed |
| `LoyaltyProgramCode` | `StringValueObject` | Same shape as `CampaignCode` | `LoyaltyProgram.Code` |
| `LoyaltyPeriod` | `ValueObject` | Same shape as `CampaignPeriod` | reserved — kept for shape-consistency even though `LoyaltyProgram` has no `TimeZone` field |

**No Value Object was created for the Reward or Distribution aggregates** — unlike every aggregate above, their phase brief gave no `ValueObjects` section, so `RewardProgram.Code`/`DistributionJob.Code` stay plain `string`. See [../aggregates/reward.md](../aggregates/reward.md)/[distribution.md](../aggregates/distribution.md) for the reconciliation note.

## Reused shared Value Objects (not duplicated locally)

- `LanguageCode` (`BuildingBlock.Domain.ValueObjects`) — used by `CampaignLocalization`.
- `Money` (`BuildingBlock.Domain.ValueObjects`) — used by `Voucher.Amount`/`Balance`, `VoucherWallet`'s three balances, `VoucherReservation.ReservedAmount`, `VoucherRedemption.RedeemedAmount`, `VoucherTransfer.Amount`, `VoucherExpiration.ExpiredAmount`, `CampaignBudget.AllocatedAmount`/`SpentAmount`. No PromotionService-local Money VO was created — the shared one (deliberately currency-less, per Payment Service's own precedent) fits exactly; `Currency`/`CurrencyCode` stays a separate scalar wherever Money is used.

## Reconciliation: root entities keep flat scalar period/limit fields

`Campaign`/`Promotion`/`Coupon`/`Voucher` all keep `StartTime`/`EndTime`/`TimeZone` (and `Coupon`'s `MaxUsage`/`MaxUsagePerUser`) as plain scalar properties, not wrapped in their matching `*Period`/`CouponUsageLimit` VO — each phase brief's Properties list specified them as separate flat fields. The VOs exist and are used wherever a *reusable* bundled shape is actually needed elsewhere (`CampaignSchedule.Period`), or reserved for a future need if none exists yet.
