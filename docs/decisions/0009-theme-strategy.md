# 0009 — next-themes for Dark/Light/System

**Status:** Accepted

**Date:** 2026-07-20

## Context

Dark/Light/System theming is a required feature from the start. shadcn/ui's components are already built expecting CSS-variable-based theme switching, which is exactly the model `next-themes` implements.

## Decision

Use `next-themes` for theme state (mounted once in the root layout), with Tailwind's `darkMode: 'class'` and shadcn's existing CSS-variable convention. The theme value itself is not duplicated into a Zustand store.

## Rationale

- `next-themes` already solves the hard parts specific to Next.js: avoiding flash-of-incorrect-theme on load, syncing with `prefers-color-scheme` for "system," and persisting the user's explicit choice — reimplementing this by hand would just be recreating `next-themes` with more bugs.
- shadcn/ui ships already wired for exactly this pattern, so there's no adapter layer needed.
- Not duplicating the theme value into Zustand avoids two sources of truth for the same single piece of state — consistent with the state-ownership principle in [0006](./0006-state-management-split.md), applied here even though `next-themes` isn't Zustand or TanStack Query.

## Alternatives considered

- **Hand-rolled React Context for theme**: rejected — `next-themes` already solves the flash-of-incorrect-theme and system-preference-sync problems; a hand-rolled version would need to solve the same problems with more code and more risk of subtle bugs (e.g. SSR/hydration mismatch on initial theme).
- **Store theme value in Zustand instead of/in addition to next-themes**: rejected — redundant source of truth for one value that `next-themes` already owns and persists.

## Consequences

- Any component needing theme-aware behavior beyond CSS (e.g. swapping a logo image) reads `useTheme()` from `next-themes` directly, not a wrapped store.
- Preferences that aren't the theme value itself but are theme-_adjacent_ (e.g. a future "reduce motion" toggle) can live in a Zustand store, since that's genuinely separate state — see [state/zustand-strategy.md](../state/zustand-strategy.md).

## Related documents

[frontend/theming.md](../frontend/theming.md), [decisions/0006-state-management-split.md](./0006-state-management-split.md)
