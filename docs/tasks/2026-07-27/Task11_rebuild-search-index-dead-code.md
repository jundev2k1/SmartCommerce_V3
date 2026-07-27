# Task 11: `RebuildProductSearchIndex` frontend client is dead code

**Status:** Open.

## Source

Full-system business-requirements audit, 2026-07-27.

## Current state

`services/product/rebuild-product-search-index.ts:9` exists but is never called from any component — no admin button triggers it anywhere in the UI.

## Suggested acceptance criteria

- Either add a working admin action (e.g. a "Rebuild search index" button somewhere in Product admin screens) that calls it, or remove the unused client code if it's not actually needed yet.

**Cross-ref:** SimpleShop `docs/tasks/2026-07-27/Task11_rebuild-search-index-auth-undocumented.md` (related — the endpoint's auth requirement is also undocumented on the backend).
