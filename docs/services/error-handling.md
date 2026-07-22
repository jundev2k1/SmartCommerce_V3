# Error Handling Strategy

**Purpose:** How API errors, form validation errors, and unexpected runtime errors are each surfaced, so error UX is consistent across every feature.

**Scope:** Error typing and presentation. Loading-state UI is covered alongside this where relevant; full loading rules live in [frontend/routing.md](../frontend/routing.md) and [state/query-strategy.md](../state/query-strategy.md).

**Related documents:** [api-layer.md](./api-layer.md), [state/query-strategy.md](../state/query-strategy.md), [frontend/forms.md](../frontend/forms.md), [frontend/routing.md](../frontend/routing.md)

**When to read:** Adding any code path that can fail — API calls, forms, or anything wrapped in a route error boundary.

**When to ignore:** Working on code with no failure modes (pure presentational components, constants).

---

## Three kinds of error, three handlers

| Error kind               | Source                                                            | Handled by                                                                                                  |
| ------------------------ | ----------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| API/business error       | Backend rejects a request (validation, not-found, conflict, etc.) | Typed `ApiError`, surfaced via `shared/ui` toast, or inline for form-submit errors                          |
| Form validation error    | Zod schema fails client-side, before a request is even sent       | RHF + Zod, rendered inline by `shared/forms`' `<FormField>` — see [frontend/forms.md](../frontend/forms.md) |
| Unexpected/runtime error | Bug, network failure, unhandled exception                         | Nearest route-level `error.tsx` boundary — see [frontend/routing.md](../frontend/routing.md)                |

## `ApiError` shape

```ts
export interface ApiError {
  code: string; // backend-defined error code, once available — TBD, see api/README.md
  message: string; // user-displayable fallback message
  status: number; // HTTP status
}
```

Produced by the Axios response interceptor described in [api-layer.md](./api-layer.md). Until the real backend error-code catalog exists, mock services throw a minimal version of this shape so downstream handling code doesn't need to change once real codes land.

## Where errors surface

- **List/detail queries**: `isError`/`error` from the TanStack Query hook renders an inline error state in the feature's component (via a `shared/ui` `ErrorState` component), not a toast — the user is already looking at that part of the page.
- **Mutations** (create/update/delete): `onError` in the mutation hook triggers a toast via `shared/ui`, since the user's attention may have moved on (e.g. closed a dialog) before the response returns.
- **Route-level unexpected errors**: caught by `error.tsx`, rendering a generic "something went wrong" screen with a retry action — never a raw stack trace in production.

## What NOT to do

Don't `try/catch` around a `.service.ts` call from inside a component — that bypasses the query/mutation hook's error state and toast wiring described above. Let TanStack Query own the error path; components only read `isError`/`error` from the hook.
