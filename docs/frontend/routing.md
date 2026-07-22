# Routing (App Router)

**Purpose:** Rules for what may and may not live inside `src/app`.

**Scope:** Routing structure only — business logic rules live in [feature-modules.md](./feature-modules.md).

**Related documents:** [architecture/overview.md](../architecture/overview.md), [architecture/folder-structure.md](../architecture/folder-structure.md), [feature-modules.md](./feature-modules.md), [decisions/0002-app-router-thin-routing.md](../decisions/0002-app-router-thin-routing.md)

**When to read:** Adding a new route/page, or restructuring URLs.

**When to ignore:** Working purely inside an existing feature's internals with no routing change.

---

## Rule

A file under `src/app` may contain: route params/segment config, `layout.tsx`/`page.tsx`/`loading.tsx`/`error.tsx`/`not-found.tsx`, metadata exports, and a call into a feature's exported screen component. It must not contain: data fetching logic, form logic, business state, or any JSX beyond composing feature components into a layout.

```tsx
// src/app/(admin)/products/page.tsx
import { ProductsPage } from '@/features/products';

export default function Page() {
  return <ProductsPage />;
}
```

All the actual work — fetching, table, forms — lives in `features/products` and is exported via its `index.ts`. See [feature-modules.md](./feature-modules.md).

## Route groups

- `(auth)` — unauthenticated routes (login).
- `(admin)` — authenticated admin shell routes, wrapped by the shell layout described in [architecture/folder-structure.md](../architecture/folder-structure.md).

Route groups exist purely to apply different layouts without affecting the URL — they carry no business meaning.

## Loading & error boundaries

Every route segment that fetches data on initial load should have a `loading.tsx` rendering a skeleton from `shared/ui`, and rely on the nearest `error.tsx` boundary for unrecoverable errors. In-page fetch/mutation states (after initial load) are handled by TanStack Query's `isLoading`/`isFetching`/`isError`, not by route-level boundaries — see [state/query-strategy.md](../state/query-strategy.md).

## Root layout responsibilities

`src/app/layout.tsx` is the one place allowed to compose global providers: `ThemeProvider` (next-themes), `QueryClientProvider` (TanStack Query), and the next-intl provider. See [theming.md](./theming.md), [i18n.md](./i18n.md), and [state/query-strategy.md](../state/query-strategy.md).
