# Tech Stack

**Purpose:** The concrete set of technologies used in this project and the role each one plays.

**Scope:** What's used, not how it's organized (see [folder-structure.md](./folder-structure.md)) or why architectural patterns were chosen on top of it (see [decisions/](../decisions/template.md)).

**Related documents:** [decisions/template.md](../decisions/template.md) (all ADRs reference back here), [frontend/i18n.md](../frontend/i18n.md), [frontend/theming.md](../frontend/theming.md)

**When to read:** Setting up tooling, or checking whether a library is already part of the approved stack before introducing a new one.

**When to ignore:** Once the stack is memorized and you're doing routine feature work — this doc doesn't change often.

---

| Concern              | Technology                          | Notes                                                                                                                                                       |
| -------------------- | ----------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Framework            | Next.js (App Router, latest stable) | `src/` directory layout. `app/` is routing-only — see [architecture/overview.md](./overview.md).                                                            |
| Language             | TypeScript                          | Strict mode. No TS `enum` — see [conventions/naming.md](../conventions/naming.md).                                                                          |
| Package manager      | yarn                                | Standardized for the whole repo; do not mix with npm/pnpm lockfiles.                                                                                        |
| Styling              | Tailwind CSS v4                     | Dark mode via `@custom-variant dark` in `globals.css` (v4 has no `tailwind.config.ts` theme key), toggled by next-themes adding a `dark` class to `<html>`. |
| Component primitives | shadcn/ui + Radix UI                | Never imported directly by feature code — always through `shared/ui`. See [frontend/ui-components.md](../frontend/ui-components.md).                        |
| HTTP client          | Axios                               | Single instance in `shared/lib/api`. See [services/api-layer.md](../services/api-layer.md).                                                                 |
| Server state         | TanStack Query                      | Owns all server data, caching, pagination, cursor lists. See [state/query-strategy.md](../state/query-strategy.md).                                         |
| Client/UI state      | Zustand                             | Cross-route client state only (session cache, theme, global modal registry). See [state/zustand-strategy.md](../state/zustand-strategy.md).                 |
| Forms                | React Hook Form + Zod               | Wrapped by `shared/forms` — business code never touches RHF directly. See [frontend/forms.md](../frontend/forms.md).                                        |
| Tables               | TanStack Table                      | Manual pagination mode, wired to TanStack Query.                                                                                                            |
| Realtime             | SignalR (`@microsoft/signalr`)      | Single hub connection, feeds TanStack Query cache. See [realtime/signalr-strategy.md](../realtime/signalr-strategy.md).                                     |
| Theming              | next-themes                         | Dark / Light / System, from Phase 1 onward. See [frontend/theming.md](../frontend/theming.md).                                                              |
| i18n                 | next-intl                           | One locale (`en`) implemented now; structure is locale-routing-ready. See [frontend/i18n.md](../frontend/i18n.md).                                          |

## Deliberately not yet decided

- **Testing framework** (unit/integration/e2e) — not part of the originally specified stack. To be decided in a later phase once there's real feature code to test against; do not assume Jest/Vitest/Playwright until an ADR is written.
- **Lint import-boundary enforcement** (e.g. `eslint-plugin-boundaries`) for feature isolation — noted as a TODO in [conventions/imports-and-boundaries.md](../conventions/imports-and-boundaries.md), to be wired up in Phase 1 once the folder structure exists to enforce.
- **API client/DTO generation** from OpenAPI — placeholder only, see [api/README.md](../api/README.md). No tool chosen until the backend spec is available.
