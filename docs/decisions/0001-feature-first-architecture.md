# 0001 — Feature-First Architecture

**Status:** Accepted

**Date:** 2026-07-20

## Context

This is an enterprise admin dashboard expected to grow to 15+ business modules (auth, users, products, variants, categories, tags, search, cart, checkout, orders, inventory, warehouses, stock, notifications, audit) over many independent implementation sessions. The default Next.js pattern of putting business logic directly inside `app/` route folders works for small apps but breaks down at this scale: route folders mix routing concerns with data-fetching/forms/tables, and there's no natural home for logic that's shared across a module's nested routes but isn't global.

## Decision

Organize all business logic by domain under `src/features/<name>/`, not by route. `src/app/` contains routing concerns only (see [0002](./0002-app-router-thin-routing.md)).

## Rationale

- A feature's boundary is a folder, not a scattered set of route segments — it can be reasoned about, reviewed, or extracted in isolation.
- Adding a new module is purely additive (`features/warehouses/`) and never requires touching existing route folders.
- Route restructuring (URL changes) never requires moving business logic, only updating the thin page that imports it.
- Matches how large Flutter/React Native apps separate navigation from screens/business logic — a pattern already proven at this scale in mobile enterprise apps.

## Alternatives considered

- **Route-colocated logic** (Next.js tutorial default): rejected — doesn't scale past a handful of pages, discussed above.
- **Layer-first** (`components/`, `hooks/`, `services/` at top level, grouped by technical layer instead of domain): rejected — as module count grows, every layer folder becomes a flat list of 15+ unrelated files, and a single feature's related pieces are scattered across the whole layer tree instead of colocated.

## Consequences

- Every new feature must follow the standard anatomy in [frontend/feature-modules.md](../frontend/feature-modules.md) — deviation is a bigger cost here than in a smaller project because consistency across dozens of features is what makes navigation-by-convention work.
- Cross-feature dependencies must go through public barrels — see [conventions/imports-and-boundaries.md](../conventions/imports-and-boundaries.md).

## Related documents

[architecture/overview.md](../architecture/overview.md), [architecture/folder-structure.md](../architecture/folder-structure.md), [frontend/feature-modules.md](../frontend/feature-modules.md)
