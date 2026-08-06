# Phase 4 — Search Integration

## Purpose

Add Elasticsearch integration for Promotion documents following [../search/search-strategy.md](../search/search-strategy.md), mirroring the existing Product/User ES architecture already established on the platform — no new ES pattern invented for this service.

## Expected Output

- `PromotionSearchDocument` mapping + index configuration.
- Indexer wiring (projection builder + sync events, matching the existing Product/User pattern).
- `IPromotionSearchService` implementation: search, autocomplete, fuzzy search.

## Build Verification

`Promotion.Infrastructure`/`Promotion.Persistence` build with the ES client wired; index configuration validated against a local ES instance where available.

## Completion Criteria

Promotion documents are indexable and searchable end-to-end in isolation from CQRS — no dependency on Phase 5's handlers yet (Phase 5 is what wires search into an actual query endpoint).

## Blocked Items

Search-backed query endpoints (the actual API surface) belong to Phase 5/6, not this phase.

## Dependencies

Phase 3 (Persistence) — the indexer needs the entities/sync events Phase 3's schema produces.
