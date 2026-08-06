# Task 3: Promotion Service — Phase 2.5 Domain Standardization Review

**Status:** Done — this closes Phase 2 (Domain Model). Phase 3 (Persistence) is current.
**Category:** Domain-layer review/refactor (consolidation, renaming, Translation standardization), no business logic, first real build

## What was done

**Duplication removal (the review's main deliverable):**
- **7 Code Value Objects → `EntityCode`**: `CampaignCode`, `PromotionCode`, `CouponCode`, `VoucherCode`, `LoyaltyProgramCode`, `RecommendationCode`, `ProductSetCode` were byte-for-byte identical and are now one shared `EntityCode` in `Promotion.Domain/ValueObjects/EntityCode.cs`. This also **removed the `CouponCode` entity/Value Object name collision** from Phase 2.2 at its root — the VO no longer shares the entity's name, so the file-local (`CouponCodeVo`) and global (`CouponCodeEntity`) alias workarounds were deleted entirely.
- **6 Period Value Objects → `Period`**: `CampaignPeriod`, `PromotionPeriod`, `CouponPeriod`, `VoucherPeriod`, `LoyaltyPeriod`, `RecommendationPeriod` were identical (`StartTime`/`EndTime`/optional `TimeZone`) and are now one shared `Period` (`TimeZone` made nullable to accommodate `LoyaltyProgram`/`RecommendationProgram`, which never had one).
- **4 Program status enums → `ProgramStatus`**: `LoyaltyProgramStatus`, `RewardProgramStatus`, `RecommendationProgramStatus`, `GiftProgramStatus` were identical (`Draft`/`Active`/`Paused`/`Expired`/`Archived`, same order) and are now one shared `ProgramStatus`.
- 17 duplicate files deleted; every affected entity (`Campaign`, `CampaignSchedule`, `Promotion`, `Coupon`, the `CouponCode` entity, `Voucher`, `LoyaltyProgram`, `RewardProgram`, `RecommendationProgram`, `ProductSet`, `GiftProgram`) updated to reference the consolidated types. `CouponUsageLimit`/`PromotionPriorityValue` were reviewed and kept separate — genuinely unique shapes, no merge candidate.
- Decided **not** to merge these VOs/enums *across* the platform's existing pattern of per-bounded-context duplication (e.g. Payment's local `Money`) — this consolidation is intra-project (all 13 lived in the same `Promotion.Domain` assembly), which the brief's "no duplicated value objects" + "keep every aggregate independent" pair of rules both permit once read as "don't duplicate within one project, don't couple aggregates transactionally" rather than "never share a primitive shape."

**Naming consistency:**
- `CampaignLocalization` → `CampaignTranslation` (only entity in the whole service using "Localization" instead of "Translation" — every other Translation-pattern precedent in the platform, `ProductBrandTranslation`/`RoleTranslation`/etc., uses "Translation"). `Campaign.Localizations` → `Campaign.Translations`.

**Translation Standardization — added to 9 more aggregates** (Campaign already had one, just renamed):
`PromotionTranslation`, `CouponTranslation`, `VoucherTranslation`, `LoyaltyProgramTranslation`, `RewardProgramTranslation`, `GiftProgramTranslation`, `RecommendationProgramTranslation`, `ProductSetTranslation`, `ProductBundleTranslation` — each follows the identical shape (`Id` reuses parent's `Id`, composite `(Id, LanguageCode)`, `internal Create`/`UpdateDetails`) and each owning entity gets exactly one `Translate(languageCode, name, description)` upsert method (find-existing→update, else create) — never `AddTranslation`/`UpdateTranslation`/`PatchTranslation`/`ReplaceTranslation`, per the brief's explicit instruction. `ProductBundle` is the one exception to "root only" — it's a non-root child of `ProductSet`, but was explicitly named as a translatable example in the brief, so it got its own `Translate(languageCode, name)` (Name-only — it has no `Description` field) directly on the class, reachable via `productSet.Bundles`.

**Explicitly not made translatable** (per the brief's own exclusion criteria — "internal processing, execution, history, logs, ledgers, audit, scheduling, or infrastructure"): every child/history/execution/ledger entity across all 13 groups, plus `DistributionJob`, `ApprovalWorkflow`, and the Validation/Audit entity groups (none are customer-facing configuration).

**`LoyaltyProgram` inferred as translatable**, though not in the brief's explicit example list (Campaign, Promotion, Coupon, Voucher, Reward Program, Gift Program, Recommendation Program, Product Set, Bundle) — it's structurally identical to `RewardProgram`/`GiftProgram` (both listed) and clearly customer-facing, so treated as a "typical example" omission rather than a deliberate exclusion. Flagged for confirmation, same as every other inference this roadmap made.

**Translation Implementation Workflow documented**: [docs/promotion-service/entities/translation-workflow.md](../../promotion-service/entities/translation-workflow.md) — the Domain-layer shape (reference) plus the frozen 13-step future Translation API order (Minimal API Endpoint → Request DTO → Response DTO → Command → Validator → Mapster Configuration → Handler → Write Persistence Service method → Repository logic → `Aggregate.Translate(...)` → Mapping Response → Build → Commit), exactly as given in the brief.

**No `{Entity}TranslationData` wrapper class created anywhere** — the brief asked for one literally, but "following the existing project format" (also explicitly required) has no such pattern anywhere in the codebase (`ProductBrandTranslation`/`RoleTranslation`/etc. all take flat parameters), and creating one would violate this same prompt's own "Do NOT introduce new patterns" instruction and Domain Rule 2. `Translate(...)` takes flat parameters directly, matching every real precedent.

**Persistence/CQRS scope respected**: no repository, persistence service, or read service method was added anywhere — the brief's "intentionally keeps these almost empty" rule was honored exactly as stated.

**Build**: `dotnet build src/Services/Promotion/Promotion.Domain/Promotion.Domain.csproj` — the Domain-only build the brief required, not the full solution. **Succeeded, 0 errors** on the first attempt, validating every manual signature cross-check made across Tasks 5-8/1-2 of this roadmap. Only the 4 pre-existing `NU1510` warnings (unrelated to Promotion) remained. Final counts (verified by file count, not estimated): **103 entities, 21 enums, 4 PromotionService-local Value Objects + 1 Metadata Value Object.**

## Objective

Freeze the Domain model before Persistence (Phase 3) begins — review, standardize, deduplicate, and add the platform-standard Translation capability, without adding any new business feature or architecture.

## Current state (grounded findings)

- `Promotion.Domain` now compiles clean: **103 entities** (94 at end of Phase 2.4 + 9 new Translation entities added this task; `CampaignTranslation` is a rename of the pre-existing `CampaignLocalization`, net zero), **21 enums** (24 minus 4 merged into `ProgramStatus`, plus 1), **4 PromotionService-local Value Objects** (15 minus 13 deleted — 7 Code + 6 Period — plus 2 created — `EntityCode` + `Period`) + 1 Metadata Value Object. All four counts verified by file count (`find Entities|Enums|ValueObjects -name "*.cs" | wc -l`), not estimated — the TODO.md/README "94 entities/24 enums/15 VOs" figures from Phase 2.4 are now superseded and corrected in this task's doc updates.
- Full exhaustive line-by-line re-review of all 94 pre-existing entities against every one of the 20 listed review criteria (region organization, XML docs, internal constructors, encapsulation, ...) was **not** performed — this task targeted the concrete, verifiable duplication/naming issues identified across the prior 5 implementation tasks' own reconciliation notes, plus the explicit Translation requirement, and relied on the Domain build to catch structural regressions. Flagging this scope choice explicitly rather than claiming an audit that wasn't done.

## Scope

**Built/changed this task:** VO/enum consolidation (17 files deleted, 2 created), 1 rename, 9 new Translation entities + `Translate()` methods, 1 new workflow doc, 1 verification build.

**Explicitly not built:** any repository/persistence-service/read-service method; any business rule; any Persistence/CQRS/API/Search code; any new pattern not already present in the codebase.

## Dependencies

Phase 2.4 (Task 2, `2026-08-07`). Phase 3 (Persistence) depends on this frozen Domain model.

## Estimated complexity

Large (cross-cutting refactor touching 11 previously-implemented entity files plus 19 new/changed files, one full-project build).

## Risks

- The Domain model is now declared frozen — any defect found during Phase 3 (Persistence) modeling will require an explicit "this is a design defect, not a new feature" justification to reopen Domain, per the brief's own closing instruction.
- The "no `TranslationData` wrapper" reconciliation is a direct, explicit divergence from one literal instruction in the brief, justified by a competing literal instruction in the same brief ("Do NOT introduce new patterns") plus the binding platform convention. If the architect actually wants a wrapper type despite it appearing nowhere else in the codebase, that is a one-time, additive, non-breaking change to make later.
- `LoyaltyProgram`'s translatability was inferred, not given explicitly — same caveat as every other inference this roadmap has made, flagged for confirmation before it's load-bearing for anything (e.g. before Persistence adds a unique index assuming translation exists).
