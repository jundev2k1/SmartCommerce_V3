# Imports & Feature Boundaries

**Purpose:** Define exactly what's allowed to import what, so features stay isolated and refactors stay cheap as the module count grows.

**Scope:** Import rules and boundary enforcement. Not naming (see [naming.md](./naming.md)) or physical folder layout (see [architecture/folder-structure.md](../architecture/folder-structure.md)).

**Related documents:** [architecture/overview.md](../architecture/overview.md), [architecture/folder-structure.md](../architecture/folder-structure.md), [naming.md](./naming.md)

**When to read:** Whenever a file needs to import something from another feature or from `shared/`, or when unsure if an import is allowed.

**When to ignore:** Pure within-feature work that only imports from the same feature folder.

---

## The dependency direction

```
app  →  features  →  shared
```

Arrows point one way only. A lower layer never imports from a higher one:

- `shared/*` never imports from `features/*` or `app/*`.
- `features/*` never imports from `app/*`.
- `app/*` may import from both `features/*` and `shared/*`.

## Cross-feature imports

A feature may depend on another feature only through its public barrel:

```ts
// OK — checkout depends on cart's public API
import { useCartItems } from '@/features/cart';

// NOT OK — reaching into cart's internals
import { useCartItems } from '@/features/cart/hooks/useCartItems';
```

If a feature needs something from another feature that isn't exported, that's a signal the shared piece belongs in `shared/` instead — move it there rather than widening the barrel to leak internals.

## Within a feature

Files inside `features/<name>/` may freely import from each other by relative or `@/features/<name>/...` path — the barrel rule only applies to _external_ consumers of the feature.

## Shared UI/forms boundary

Business/feature code never imports shadcn-generated primitives or Radix directly — always through `shared/ui` or `shared/forms`. See [frontend/ui-components.md](../frontend/ui-components.md) and [frontend/forms.md](../frontend/forms.md) for why.

```ts
// NOT OK from feature code
import { Button } from '@/components/ui/button'; // raw shadcn output

// OK
import { AppButton } from '@/shared/ui';
```

## Enforcement status

These rules are currently convention-only (reviewed by hand). Wiring `eslint-plugin-boundaries` (or an equivalent import-restriction ESLint config) to enforce them automatically is a Phase 1 TODO — see [architecture/tech-stack.md](../architecture/tech-stack.md#deliberately-not-yet-decided). Until that lint rule exists, treat this document as the binding contract during code review.
