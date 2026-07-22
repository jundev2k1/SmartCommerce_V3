# Feature Module Anatomy

**Purpose:** What goes inside a `features/<name>/` folder and how its pieces relate to each other.

**Scope:** Internal structure of a single feature. Cross-feature rules are in [conventions/imports-and-boundaries.md](../conventions/imports-and-boundaries.md); physical tree is in [architecture/folder-structure.md](../architecture/folder-structure.md).

**Related documents:** [architecture/folder-structure.md](../architecture/folder-structure.md), [conventions/naming.md](../conventions/naming.md), [state/overview.md](../state/overview.md), [ui-components.md](./ui-components.md), [forms.md](./forms.md)

**When to read:** Starting a new feature module, or unsure where a piece of new logic belongs within an existing one.

**When to ignore:** Working on `shared/` or `app/` code.

---

## Standard shape

```
features/<name>/
├── components/     Feature-scoped UI, built from shared/ui primitives
├── hooks/          Feature-scoped non-query business hooks
├── api/
│   ├── <name>.service.ts     Calls the shared Axios client, returns business data only
│   └── <name>.queries.ts     TanStack Query hooks + query-key factory for this feature
├── store/          Zustand store, ONLY if the feature has genuine cross-component client state
├── <name>.types.ts
├── <name>.schema.ts
└── index.ts        Public barrel
```

Omit folders that don't apply. A read-only, list-only feature (e.g. audit) may skip `store/` and most of `hooks/` entirely.

## Responsibilities per piece

- **`components/`** — presentation + local interaction only. Data comes in via props or the feature's query hooks; components don't call `.service.ts` directly.
- **`hooks/`** — business logic that isn't server-state (e.g. `useProductFilters` managing filter UI state) or that composes multiple query hooks together.
- **`api/*.service.ts`** — thin functions calling the shared Axios instance (`@/shared/lib/api`), returning already-unwrapped business data. Never returns Axios responses or the raw envelope — see [services/api-layer.md](../services/api-layer.md).
- **`api/*.queries.ts`** — TanStack Query `useQuery`/`useMutation`/`useInfiniteQuery` hooks wrapping the service functions, plus a query-key factory. This is the only place `.service.ts` functions are called from.
- **`store/`** — Zustand, only for state that must survive across components/routes within the feature and isn't server data. Most features need none. See [state/zustand-strategy.md](../state/zustand-strategy.md).
- **`<name>.types.ts` / `<name>.schema.ts`** — DTOs/view models and Zod schemas, per [conventions/naming.md](../conventions/naming.md).
- **`index.ts`** — the feature's public API. Typically exports the top-level screen component used by `app/`, and occasionally a hook/type a dependent feature legitimately needs.

## Data flow within a feature

```
component  →  query hook (api/*.queries.ts)  →  service (api/*.service.ts)  →  shared Axios client
```

Components never call `.service.ts` directly — always go through the query hook layer so caching, loading states, and invalidation are consistent. See [state/query-strategy.md](../state/query-strategy.md).
