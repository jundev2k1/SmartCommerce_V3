# Folder Structure

**Purpose:** Exact directory tree for the project, down to the anatomy of a single feature module.

**Scope:** Physical file/folder layout only. For _why_ this layout was chosen, see [overview.md](./overview.md) and [decisions/0001-feature-first-architecture.md](../decisions/0001-feature-first-architecture.md). For naming rules within these folders, see [conventions/naming.md](../conventions/naming.md).

**Related documents:** [overview.md](./overview.md), [conventions/naming.md](../conventions/naming.md), [conventions/imports-and-boundaries.md](../conventions/imports-and-boundaries.md), [frontend/feature-modules.md](../frontend/feature-modules.md)

**When to read:** Whenever creating a new file/folder and unsure exactly where it goes.

**When to ignore:** If you already know the target feature exists and just need to add a file inside it — check that feature's existing structure directly instead.

---

## Top-level tree

```
SimpleShopUI/
├── docs/                       (this documentation system)
├── public/
├── src/
│   ├── app/                    Routing only
│   │   ├── (auth)/
│   │   │   └── login/page.tsx
│   │   ├── (admin)/
│   │   │   ├── layout.tsx      Admin shell (sidebar/topbar), renders children
│   │   │   ├── users/page.tsx
│   │   │   ├── products/page.tsx
│   │   │   └── ...             one thin page per route
│   │   ├── layout.tsx          Root layout: ThemeProvider, QueryClientProvider, IntlProvider
│   │   ├── error.tsx
│   │   └── loading.tsx
│   ├── features/
│   │   └── <feature-name>/     e.g. auth, users, products, product-variants, cart, orders...
│   │       ├── components/     Feature-scoped UI (uses shared/ui, never business logic elsewhere)
│   │       ├── hooks/          Feature-scoped hooks (non-query business hooks)
│   │       ├── api/
│   │       │   ├── <feature>.service.ts       Calls shared Axios client, returns business data only
│   │       │   └── <feature>.queries.ts       TanStack Query hooks + query-key factory
│   │       ├── store/          Feature-scoped Zustand store, only if genuinely needed (rare)
│   │       ├── <feature>.types.ts
│   │       ├── <feature>.schema.ts             Zod schemas for this feature's forms
│   │       └── index.ts        Public barrel — the ONLY thing other features/app may import
│   ├── services/                Backend-service-shaped API clients — see decisions/0012-backend-service-client-layer.md
│   │   ├── shared/
│   │   │   └── paginated-result.ts   BackendPaginatedResult<T>, reused across services
│   │   └── <service-name>/      e.g. audit, auth, user, order, inventory, product, notification
│   │       ├── _base.ts         BASE_PATH constant (matches that service's Swagger servers.url)
│   │       ├── <operation>.ts   One file per endpoint, named after the Swagger operationId — declares its own request/response DTOs inline (named after the Swagger schema)
│   │       ├── types/           Opaque enum-like domain vocabulary shared across endpoints (e.g. OrderStatus) — not request/response DTOs
│   │       └── index.ts         Barrel
│   ├── shared/
│   │   ├── ui/                 Wrapped shadcn/Radix primitives — see frontend/ui-components.md
│   │   ├── entity/               Reusable admin-CRUD-screen scaffolding (EntityHeader, EntityToolbar, ConfirmDeleteDialog, AuditTrailButton, ...) — see decisions/0014-shared-entity-component-layer.md
│   │   ├── commerce/             Reusable customer-facing commerce components (ProductCard, VariantSelector, CartSummary, OrderStatusBadge, ...) — see decisions/0015-shared-commerce-component-layer.md
│   │   ├── inventory/            Reusable Inventory-domain components (WarehouseSelector, InventorySummaryCard, StockStatusBadge, TransactionTimeline, ...) — see decisions/0016-shared-inventory-component-layer.md
│   │   ├── notifications/        Reusable notification-feed presentation (NotificationItem, NotificationList, UnreadBadge, ...) — see decisions/0017-shared-notifications-component-layer.md
│   │   ├── forms/               Internal form components/hooks — see frontend/forms.md
│   │   ├── layout/               Admin shell chrome: AdminShell, Sidebar, Topbar, Breadcrumb, UserMenu, NotificationBell, ThemeToggle
│   │   ├── lib/
│   │   │   ├── api/            Axios client, interceptors — see services/api-layer.md
│   │   │   ├── realtime/       SignalR client — see realtime/signalr-strategy.md
│   │   │   └── utils/          Generic, feature-agnostic helpers only
│   │   ├── stores/             Global Zustand stores (theme, session cache, global modal registry)
│   │   ├── hooks/               Generic cross-feature hooks (e.g. useDisclosure, useCursorList)
│   │   ├── types/               Cross-feature shared types only (avoid dumping everything here)
│   │   └── config/               Nav config, env config, app-wide constants
│   ├── i18n/
│   │   ├── config.ts
│   │   └── messages/
│   │       └── en/
│   │           ├── common.json
│   │           └── <feature>.json   One namespace file per feature
│   └── (styling lives in app/globals.css, above — Tailwind v4 entry + theme tokens; no separate styles/ dir, per Next.js/shadcn convention)
├── components.json                shadcn CLI config (aliases point ui→components/ui, utils/lib/hooks→shared/*)
├── package.json
├── tsconfig.json                (path alias "@/*" → "src/*")
└── next.config.ts
```

## Anatomy of a feature module

Every entry under `src/features/` follows the same shape (omit subfolders that don't apply — e.g. a feature with no local state skips `store/`):

```
features/products/
├── components/
│   ├── ProductTable.tsx
│   ├── ProductForm.tsx
│   └── ProductStatusBadge.tsx
├── hooks/
│   └── useProductFilters.ts
├── api/
│   ├── products.service.ts
│   └── products.queries.ts
├── product.types.ts
├── product.schema.ts
└── index.ts
```

`index.ts` exports only what `app/` or other features are allowed to consume (typically the top-level page component, and occasionally a hook or type another feature genuinely needs). Everything else in the folder is private to the feature — see [conventions/imports-and-boundaries.md](../conventions/imports-and-boundaries.md) for the enforcement rule.

## Path alias

`@/*` maps to `src/*` (configured in `tsconfig.json`), so imports read as `@/features/products`, `@/shared/ui/button`, etc. — never relative-path across top-level layers.
