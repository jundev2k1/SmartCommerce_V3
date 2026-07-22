# 0014 — `shared/entity/`: Reusable Admin-CRUD-Screen Scaffolding

**Status:** Accepted

**Date:** 2026-07-20 (Phase 3 — Catalog)

## Context

Catalog is the first phase with four modules (Products, Categories, Tags, and Product Search) needing the same admin-screen shapes: a title+actions header, a search/filter/bulk-actions toolbar, a delete confirmation, a status badge, a metadata grid, and an audit-trail viewer. Building these once per module would violate the brief's own "avoid duplicated UI or business logic" instruction, and every future bounded context (Inventory, Orders, a revisited Users) will want the same pieces.

None of it fits the two existing shared categories: `shared/ui` wraps third-party primitives (ADR 0003); `shared/layout` is admin-shell chrome (ADR 0012's sibling decision). This is a third, distinct category: reusable _business-screen scaffolding_ — presentational and composition patterns for building CRUD screens, with no third-party dependency of its own beyond what `shared/ui` already wraps.

## Decision

Add `shared/entity/`: `EntityHeader`, `EntityToolbar`, `EntityStatusBadge`, `EntityMetadata`, `ConfirmDeleteDialog`, `FilterPanel`, `SelectionPanel`, `AuditTrailButton`/`AuditTrailDialog`, `ImageUrlListField`. One barrel, consumed the same way `shared/ui`/`shared/layout` are.

`features/users` (built in Phase 2, before this layer existed) is refactored to consume `AuditTrailDialog` from here instead of its own one-off copy — the first real instance of "don't implement audit logic again" being enforced in practice.

## Rationale

- **Three-plus real call sites already exist** (Products, Categories, Tags all need a delete-confirm and a status badge on day one; Product Search reuses the toolbar/filter pieces too) — this isn't speculative abstraction, the repetition is already there before the layer exists.
- Keeps `features/*` focused on business flow, not re-deriving the same header/toolbar/confirm-dialog markup four times with subtly different implementations that drift over time.
- Distinct from `shared/ui` because these components have opinions about _admin CRUD screen structure_ (a header has a title and actions; a toolbar has search+filters+bulk-actions in a specific arrangement), not just wrapping one third-party primitive with app defaults.

## Alternatives considered

- **Duplicate the patterns per feature**: rejected — directly contradicts "avoid duplicated UI or business logic," and four near-identical delete-confirm dialogs is exactly the kind of drift this decision exists to prevent.
- **Put these in `shared/ui`**: rejected — `shared/ui` is specifically scoped (per its own doc) to third-party-wrapper components; folding in composed business-screen patterns would blur that boundary and make `shared/ui` harder to reason about as "the wrapper layer."
- **A `features/catalog` shared-within-bounded-context folder** (not promoted to `shared/`): rejected — Users (Phase 2) already needed the audit-trail piece, so scoping this to "just Catalog" would have been wrong on day one, not just eventually.

## Consequences

- Any future module needing a list/detail screen reaches for `shared/entity` first, rather than hand-rolling the same header/toolbar/confirm-delete/audit-button shapes again.
- `AppDataTable` (`shared/ui`) gained two additive, backward-compatible capabilities to support this layer: controlled row selection (`enableRowSelection`/`rowSelection`/`onRowSelectionChange`/`getRowId`, powering `SelectionPanel`) and an internal column-visibility toggle — no existing call site (Users) needed to change.
- `ImageUrlListField` manages a list of image URL strings with preview, not a file uploader — no backend service in this project exposes an upload endpoint (see `docs/backend/product/README.md`); this is documented plainly rather than built as if upload existed.

## Update (Phase 8 — Production Readiness)

Added `EntityDetailHeader` (breadcrumb + `EntityHeader`, always paired identically) after a Phase 8 consistency review found the exact same two-component composition duplicated verbatim across five detail pages (Product, Warehouse, Inventory record, Order, and the customer-facing Shop product detail) — a proven, not speculative, pattern per this ADR's own standard for adding to this layer. Same phase also caught and fixed several hardcoded English strings inside `shared/entity` components (`ImageUrlListField`'s "Move up"/"Remove image"/placeholder) that had bypassed translation despite the rest of the layer being fully translated — see [backlog.md](../backlog.md) and the Phase 8 changelog entry.

## Related documents

[architecture/overview.md](../architecture/overview.md), [architecture/folder-structure.md](../architecture/folder-structure.md), [frontend/ui-components.md](../frontend/ui-components.md), [decisions/0003-ui-wrapper-strategy.md](./0003-ui-wrapper-strategy.md), [decisions/0012-backend-service-client-layer.md](./0012-backend-service-client-layer.md), [modules/product-management.md](../modules/product-management.md), [backlog.md](../backlog.md)
