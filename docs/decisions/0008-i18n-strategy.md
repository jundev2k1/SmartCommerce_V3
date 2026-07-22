# 0008 — next-intl, Locale-Ready Structure From Day One

**Status:** Accepted

**Date:** 2026-07-20

## Context

Only one locale ships initially, but the project explicitly requires i18n to be prepared from the beginning rather than retrofitted. Retrofitting translation keys into components that were written with hardcoded strings is significantly more error-prone (easy to miss strings, easy to introduce inconsistent key naming after the fact) than writing every string through a translation call from the start.

## Decision

Use **next-intl** for translations, with messages namespaced per feature under `src/i18n/messages/<locale>/<feature>.json`. Every user-facing string goes through `useTranslations()` from the first feature built (Phase 1 onward), even though only `en` exists.

## Rationale

- next-intl has native App Router support (server and client components), unlike react-i18next which requires more manual integration work in that environment.
- Next.js's own built-in i18n routing handles locale-in-URL routing but provides no message-management story — next-intl would still be needed on top of it, making the built-in option strictly more work for no benefit.
- Per-feature message namespacing mirrors the Feature-First structure ([0001](./0001-feature-first-architecture.md)) conceptually, even though the files physically live under `src/i18n/` (a structural exception required by next-intl's expectations, not a violation of feature ownership in spirit).

## Alternatives considered

- **Next.js built-in i18n routing only**: rejected — no message file management, would need a second library anyway.
- **react-i18next**: rejected — more manual server/client component integration work under App Router compared to next-intl's purpose-built support.
- **Hardcode English now, add i18n later**: rejected — explicitly against the project's stated requirement, and the retrofit cost described above.

## Consequences

- Every new component with user-facing text must add its strings to that feature's message namespace file, not inline.
- Adding a second locale later is purely additive (new message files + locale registration), with zero component code changes — this is the entire payoff of this decision.

## Related documents

[frontend/i18n.md](../frontend/i18n.md), [architecture/tech-stack.md](../architecture/tech-stack.md)
