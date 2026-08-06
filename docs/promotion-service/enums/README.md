# Promotion Service — Enums

**Scope:** Catalogue of every `Promotion.Domain` enum, updated as each is introduced (step 6 of [../entities/entity-implementation-strategy.md](../entities/entity-implementation-strategy.md)).

| Enum | Values | Used by | Notes |
|---|---|---|---|
| `CampaignStatus` | Draft, Scheduled, Active, Paused, Completed, Cancelled | `Campaign.Status` | Lifecycle state machine, guarded by `Campaign.Activate`/`Pause`/`Complete`/`Cancel` |
| `CampaignType` | Seasonal, FlashSale, Loyalty, Acquisition, Retention, Clearance, Custom | `Campaign.Type` | **Structural placeholder** — not architect-specified, confirm before treating as final |
| `PromotionStatus` | Draft, Active, Paused, Expired, Cancelled | `Promotion.Status` | Lifecycle state machine, guarded by `Promotion.Activate`/`Pause`/`Expire`/`Cancel` |
| `PromotionType` | PercentageOff, FixedAmountOff, BuyXGetY, FreeShipping, Bundle, Custom | `Promotion.Type` | **Structural placeholder** — not architect-specified; deliberately excludes "Coupon" (its own future aggregate) |
| `PromotionPriorityType` | Low, Normal, High, Critical | `PromotionPriority.PriorityType` | Classification only, distinct from `Promotion.Priority` (plain ordering int) |
| `PromotionExecutionMode` | Automatic, Manual, Scheduled | `PromotionExecutionPolicy.Mode` | How a Promotion is triggered |
| `PromotionStackingMode` | NotStackable, StackWithAny, StackWithSameType, StackWithSpecific | `PromotionStackingPolicy.Mode` | How a Promotion combines with other active promotions |
