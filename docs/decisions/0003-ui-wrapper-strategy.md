# 0003 — Wrap All Third-Party UI Components

**Status:** Accepted

**Date:** 2026-07-20

## Context

shadcn/ui generates component source directly into the repo (not a versioned npm dependency in the usual sense), and sits on top of Radix UI primitives. Both can change API shape between "adds" and Radix major versions. With business code spread across 15+ feature modules, a direct dependency on either would mean a future upgrade or API tweak touches every call site across the whole codebase.

## Decision

Business/feature code never imports shadcn-generated primitives or Radix directly. All UI consumption goes through `src/shared/ui`, a thin wrapper layer with a single barrel export.

## Rationale

- Isolates the codebase from third-party API churn to one wrapper layer instead of every call site.
- Gives one place to add app-wide behavior (e.g. a `Button` with a built-in loading-spinner variant, consistent default props) without touching business code.
- Makes a future full replacement of shadcn/Radix (unlikely, but possible over a multi-year project) a one-layer rewrite instead of a repo-wide find/replace.

## Alternatives considered

- **Import shadcn/Radix directly everywhere**: rejected for the reasons above — this is the default/easy path but doesn't hold up over years of an enterprise codebase.
- **Wrap only components that need custom behavior, use shadcn output directly otherwise**: rejected — an inconsistent boundary is harder to enforce via review/lint than a blanket rule, and the wrapper cost for a pure re-export is negligible.

## Consequences

- Every new shadcn component addition requires a corresponding `shared/ui` wrapper file, even if it's a pure re-export initially — see [frontend/ui-components.md](../frontend/ui-components.md) for the exact steps.
- Enforcing the boundary via lint (`eslint-plugin-boundaries` or import restrictions) is a Phase 1 TODO — until then it's a code-review convention.

## Related documents

[frontend/ui-components.md](../frontend/ui-components.md), [conventions/imports-and-boundaries.md](../conventions/imports-and-boundaries.md)
