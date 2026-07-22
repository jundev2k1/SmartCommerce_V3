# Architecture Overview

**Purpose:** High-level description of how the codebase is organized and why Feature-First was chosen over the more common Next.js "everything in `app/`" pattern.

**Scope:** Top-level architectural shape only. Detailed folder layout is in [folder-structure.md](./folder-structure.md). Naming rules are in [conventions/naming.md](../conventions/naming.md).

**Related documents:** [folder-structure.md](./folder-structure.md), [tech-stack.md](./tech-stack.md), [decisions/0001-feature-first-architecture.md](../decisions/0001-feature-first-architecture.md), [decisions/0002-app-router-thin-routing.md](../decisions/0002-app-router-thin-routing.md)

**When to read:** First doc to read before starting any new feature, or when unsure where a piece of code should live.

**When to ignore:** Once you already know Feature-First and just need the exact folder tree — go straight to [folder-structure.md](./folder-structure.md) instead.

---

## The core rule

**`src/app` only does routing. `src/features` holds all business logic.**

This project is an enterprise admin dashboard expected to grow to dozens of modules (auth, users, products, variants, categories, tags, search, cart, checkout, orders, inventory, warehouses, stock, notifications, audit, and more). Putting business logic inside `app/` route folders — the default Next.js tutorial pattern — does not scale past a handful of pages: route folders become dumping grounds mixing routing concerns (layouts, loading states, metadata) with business logic (data fetching, forms, tables), and there's no natural home for logic shared across nested routes but not the whole app.

Instead, this project treats `app/` the way Flutter or React Native treats their navigation layer: a thin routing shell that renders screens, nothing more. All business logic — components, hooks, API calls, state, types — lives in `src/features/<feature-name>/`, organized by _domain_, not by _route_. A page in `app/` is typically a few lines: import the feature's top-level component/screen and render it.

See [decisions/0001-feature-first-architecture.md](../decisions/0001-feature-first-architecture.md) and [decisions/0002-app-router-thin-routing.md](../decisions/0002-app-router-thin-routing.md) for the full rationale.

## Layer map

```
src/
├── app/          Routing only — layouts, pages, loading/error boundaries, route groups
├── features/     Business logic, organized by domain (one folder per feature)
├── services/     Backend-service-shaped API clients (one folder per backend microservice, not per feature)
├── shared/       Cross-feature reusable code (ui, entity, forms, layout, lib, stores, hooks, types, config)
└── (app/globals.css holds global CSS / Tailwind entry — no separate styles/ dir)
```

- **`app/`** never imports another route's internals. It imports from `features/` and `shared/` only.
- **`features/`** modules never import from each other's internals — only through each other's public barrel (`index.ts`), and only when a real cross-feature dependency exists (e.g. `checkout` reading from `cart`'s public API). See [conventions/imports-and-boundaries.md](../conventions/imports-and-boundaries.md).
- **`services/`** sits between `shared/lib/api` (transport) and `features/` (business/UI): one folder per _backend_ service, one file per endpoint. A single backend service can back several frontend features (e.g. the Product service backs Catalog, Variants, Categories, Tags, and Search), so this layer is organized by backend contract, not by feature. See [decisions/0012-backend-service-client-layer.md](../decisions/0012-backend-service-client-layer.md).
- **`shared/`** never imports from `features/` or `services/`. Dependencies flow one direction: `app → features → services → shared`. Within `shared/`, `shared/entity` (reusable admin-CRUD-screen scaffolding — headers, toolbars, confirm-delete, audit trail, status badges) is a third category alongside `shared/ui` (third-party wrappers) and `shared/layout` (admin shell chrome) — see [decisions/0014-shared-entity-component-layer.md](../decisions/0014-shared-entity-component-layer.md). `shared/commerce` (Phase 4) is a fourth category: reusable customer-facing commerce components (product cards, price/variant/quantity pickers, cart/checkout/order summaries) — see [decisions/0015-shared-commerce-component-layer.md](../decisions/0015-shared-commerce-component-layer.md). `shared/inventory` (Phase 5) is a fifth: reusable Inventory-domain components (warehouse picker, stock summary/quantity, opaque status badge, transaction timeline), deliberately independent from Catalog — see [decisions/0016-shared-inventory-component-layer.md](../decisions/0016-shared-inventory-component-layer.md). `shared/notifications` (Phase 7) is a sixth: reusable, prop-driven notification-feed presentation (item row, list, badges, timestamp, icon, loading/empty states) consumed by `features/notifications` — see [decisions/0017-shared-notifications-component-layer.md](../decisions/0017-shared-notifications-component-layer.md).

## Why this scales

- New modules are additive: adding "warehouses" means adding `features/warehouses/`, not touching existing route folders.
- A feature can be reasoned about, tested, or even extracted in isolation because its boundary is a folder, not a scattered set of route segments.
- Route restructuring (e.g. moving a page under a different URL segment) never requires moving business logic — only updating the thin `app/` page that imports it.

See [tech-stack.md](./tech-stack.md) for the concrete technology choices that fill these layers, and [folder-structure.md](./folder-structure.md) for the exact directory tree.
