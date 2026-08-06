# Promotion Service — Translation Feature Implementation Workflow

**Scope:** The standard order every future Translation API feature follows, frozen during the Phase 2.5 Domain Standardization Review. Future prompts may reference this document instead of re-explaining the workflow. This is an Application/API-layer workflow — the Domain-layer half (the `{Entity}Translation` entity + the owning Aggregate Root's `Translate(...)` upsert method) is already complete for every translatable aggregate as of Phase 2.5; see [../aggregates/](../aggregates/) for which ones.

## Domain-layer shape (already built, reference only)

Every translatable Aggregate Root follows this exact shape — clone it when a *new* aggregate becomes translatable in a future phase:

1. `{Entity}Translation.cs` — `BaseEntity<Guid>`, `IAuditable`, `ITenantEntity`. `Id` reuses the parent's `Id` (Rule 5); composite key `(Id, LanguageCode)` configured in Persistence (Phase 3). Fields: `LanguageCode` + whatever the parent's own translatable fields are (typically `Name`/`Description`).
2. Parent root exposes `ICollection<{Entity}Translation> Translations`.
3. Parent root exposes exactly one method: `Translate(LanguageCode languageCode, string name, string? description)` — **never** `AddTranslation`/`UpdateTranslation`/`PatchTranslation`/`ReplaceTranslation`. Upsert body:
   ```csharp
   public void Translate(LanguageCode languageCode, string name, string? description)
   {
       ValidateName(name);
       var existing = Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
       if (existing is not null) { existing.UpdateDetails(name, description); return; }
       Translations.Add({Entity}Translation.Create(Id, languageCode, name, description));
   }
   ```
4. **No `{Entity}TranslationData` wrapper class** — `Translate(...)` takes flat parameters directly, per [domain-coding-conventions.md Rule 2](../../conventions/domain-coding-conventions.md#2-no-specrequestcommanddto-like-objects-inside-domain---flat-parameters-instead) and the real platform precedent (`ProductBrand.Translate`). A prompt asking for a `TranslationData` file is asking for a pattern that doesn't exist anywhere in this codebase — see the Phase 2.5 task record for the full reconciliation.

## Future Translation API implementation order

Once Persistence (Phase 3) and CQRS (Phase 5) exist for a given aggregate, implement its Translation endpoint in exactly this order:

1. Clone Minimal API Endpoint
2. Clone Request DTO
3. Clone Response DTO
4. Clone Command
5. Clone Validator
6. Clone Mapster Configuration
7. Clone Handler
8. Add method to Write Persistence Service
9. Implement Repository logic
10. Call `Aggregate.Translate(...)`
11. Mapping Response
12. Build
13. Commit

"Clone" means: copy the shape of an already-implemented Translate feature in another NovaCore service (or, once the first Promotion Translation feature ships, copy that one) rather than designing from scratch — the goal of freezing this order is that a future Translation API is clonable without re-reading each aggregate's own implementation.

## Which aggregates are translatable

See [../aggregates/README.md](../aggregates/README.md) for the full list. Translatable (10): Campaign, Promotion, Coupon, Voucher, LoyaltyProgram, RewardProgram, GiftProgram, RecommendationProgram, ProductSet, ProductBundle (the last is not an Aggregate Root — see [../aggregates/product-set.md](../aggregates/product-set.md) for how its `Translate(...)` is exposed instead). Every other entity in this service (executions, ledgers, history, audit, scheduling, infrastructure) is explicitly excluded per the Phase 2.5 brief's own criteria.
