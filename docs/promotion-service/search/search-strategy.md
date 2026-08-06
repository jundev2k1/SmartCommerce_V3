# Promotion Service — Search Strategy

**Scope:** The future Elasticsearch integration Promotion Service adopts in Phase 4. Documentation only — no index, document, or search service exists yet. This mirrors the platform's existing Product/User Elasticsearch architecture; it does not invent a new search pattern. See [../../services/product-service.md](../../services/product-service.md) and the User Service ES build-out ([../../tasks/2026-07-28/Task6_elasticsearch-scaffolding.md](../../tasks/2026-07-28/Task6_elasticsearch-scaffolding.md) through [Task10](../../tasks/2026-07-28/Task10_cutover-searchusers-to-elasticsearch.md)) as the model to copy.

## Components

- **Documents** — `PromotionSearchDocument`, a flat, denormalized projection of whatever fields Promotion's search/browse experience needs (name, description, status, date range, applicability). Built by a Projection Builder off the Domain aggregate, not a 1:1 mirror of the EF schema.
- **Index Configuration** — accent-insensitive/case-insensitive analyzer settings matching the platform convention already established for Product/User search (see [../../tasks/2026-07-28/Task7_search-document-and-accent-insensitive-mapping.md](../../tasks/2026-07-28/Task7_search-document-and-accent-insensitive-mapping.md)), plus a `RebuildPromotionSearchIndex` command mirroring [../../tasks/2026-07-28/Task9_rebuild-command-and-es-config.md](../../tasks/2026-07-28/Task9_rebuild-command-and-es-config.md) for blue/green reindex support.
- **Indexer** — sync events (internal domain events → projection → ES upsert/delete) triggered from the same transaction boundary as the aggregate write, following [../../tasks/2026-07-28/Task8_projection-builder-and-sync-events.md](../../tasks/2026-07-28/Task8_projection-builder-and-sync-events.md)'s shape.
- **Search Service** (`IPromotionSearchService`) — the query-side abstraction a Query handler depends on; wraps the ES client, never exposes NEST/Elastic.Clients types past the Infrastructure boundary.
- **Autocomplete** — prefix/edge-ngram-backed suggestion query for the admin/browse UI, same mechanism Product/User already use.
- **Fuzzy Search** — typo-tolerant matching (Levenshtein-distance fuzziness) on the primary text fields, same mechanism Product/User already use.

## Phase mapping

Phase 4 builds all of the above in isolation (indexable/searchable, no API route yet). Phase 5 is what wires a Query handler to `IPromotionSearchService` and exposes it through an endpoint — this strategy doc does not itself authorize adding that endpoint early.

## What this phase does not do

No cutover decision (ES-only vs. ES-plus-Postgres-fallback) is made here — that mirrors whatever decision the architect's design specifies; if unspecified, default to the platform's existing precedent (full cutover, per [../../tasks/2026-07-28/Task10_cutover-searchusers-to-elasticsearch.md](../../tasks/2026-07-28/Task10_cutover-searchusers-to-elasticsearch.md)) rather than inventing a hybrid approach.
