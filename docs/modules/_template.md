<!--
Template for a business module doc. Copy this file to docs/modules/<name>.md when that
module's roadmap phase actually starts. Do not create these speculatively.
-->

# `<Module Name>`

**Purpose:** _What this module does, in one or two sentences._

**Scope:** _What's in bounds for this doc — typically just this module's feature folder(s)._

**Related documents:** [modules/overview.md](./overview.md), [roadmap.md](../roadmap.md), _(link the specific architecture/state/service docs this module leans on hardest)_

**When to read:** _When implementing or modifying this module._

**When to ignore:** _Working in an unrelated module._

---

## Feature folder(s)

`src/features/<name>/` — _(list any additional related feature folders, e.g. product-variants alongside products)_

## Data model (mock, pending real backend contract)

_Zod schema / types summary — link to the actual `.schema.ts`/`.types.ts` rather than duplicating it here._

## Key flows

_e.g. list → filter → create → edit → delete; or read-only list → detail._

## Dependencies on other modules

_e.g. "Checkout reads from Cart's public API" — link to [conventions/imports-and-boundaries.md](../conventions/imports-and-boundaries.md) if this crosses a feature boundary._

## Deviations from standard conventions

_Most modules should need nothing here — only note it if this module has a genuine, deliberate exception to [conventions/](../conventions/naming.md) or [frontend/feature-modules.md](../frontend/feature-modules.md), and why._

## Open questions / backend dependencies

_What's still blocked on the real OpenAPI spec landing (see [api/README.md](../api/README.md))._
