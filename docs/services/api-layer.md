# API Layer Strategy

**Purpose:** Define the single Axios client, its interceptors, and the auth cookie/refresh-token flow, so every `src/services/<service>` endpoint file and every feature's query hook follows the same contract.

**Scope:** Transport layer only. Error typing/handling is in [error-handling.md](./error-handling.md). Query-level concerns (caching, keys) are in [state/query-strategy.md](../state/query-strategy.md). Per-service endpoint/DTO details live in [backend/](../backend/README.md), not here.

**Related documents:** [error-handling.md](./error-handling.md), [state/query-strategy.md](../state/query-strategy.md), [decisions/0005-api-layer-and-auth.md](../decisions/0005-api-layer-and-auth.md), [decisions/0012-backend-service-client-layer.md](../decisions/0012-backend-service-client-layer.md), [backend/README.md](../backend/README.md)

**When to read:** Writing or modifying any `src/services/<service>/*` endpoint file, or touching auth/refresh logic.

**When to ignore:** Pure UI or state-only work with no API interaction.

---

## Single instance

One Axios instance, created in `shared/lib/api/client.ts`, imported by every `src/services/<service>` endpoint file — nothing constructs its own Axios instance or calls `axios` directly.

```ts
export const apiClient = axios.create({
  baseURL: env.apiBaseUrl, // gateway origin only — see "Base URL" below
  withCredentials: true, // HTTP-only auth cookie
});
```

## Base URL: gateway origin only, not `/api`

`env.apiBaseUrl` is the gateway origin (empty string / same-origin by default, overridable via `NEXT_PUBLIC_API_BASE_URL`). Each backend service's Swagger `servers.url` already includes its own full relative prefix (`/api/product`, `/api/auth`, `/api/inventory`, ...), so every `src/services/<service>/_base.ts` supplies that prefix itself. `env.apiBaseUrl` must **not** also be `/api`, or paths double up (`/api/api/product/...`) — this was a Phase 1 mistake, corrected in Phase 1.5 once the real per-service Swagger prefixes were known.

## Auth: HTTP-only cookies

Authentication is cookie-based (HTTP-only, set by the backend on login) — the frontend never reads or stores the token itself. `withCredentials: true` on every request is what sends the cookie; there is no `Authorization` header to attach. (The Auth service's Swagger also declares a `Bearer` security scheme, but every endpoint's own description explicitly specifies cookie-based auth with "no tokens in response body" — the Bearer scheme is treated as vestigial backend-framework boilerplate, not implemented. See [backend/auth/README.md](../backend/auth/README.md).)

## Request interceptor: correlation ID

Every outgoing request gets a fresh `X-Correlation-Id: crypto.randomUUID()` header, alongside `Accept-Language` (from `locale.store.ts`). Only two endpoints in the current contracts formally require `X-Correlation-Id` (`auth/register`, `user` create-profile), but it's sent on every request for consistent distributed tracing rather than special-cased per endpoint.

## Response interceptor: unified envelope

Every backend service returns the same envelope shape (confirmed from real Swagger in Phase 1.5):

```ts
interface ApiResponseEnvelope<T> {
  success: boolean;
  message: string | null;
  messageCode: string | null;
  data: T;
  details: unknown | null;
}
```

The response interceptor unwraps this so callers upstream never see it:

- `success: true` → resolve with `envelope.data` only.
- `success: false` → reject with a typed `ApiError` (`{ code: messageCode, message, status }`), **even on an HTTP 200** — the backend can signal a soft failure without a non-2xx status, so the interceptor checks `envelope.success` explicitly rather than trusting the HTTP status code alone.
- A real non-2xx HTTP error goes through the same `ApiError` normalization from the Axios error path.

## Response interceptor: 401 → refresh → retry

```
request fails with 401
  → if a refresh is already in flight, queue this request behind it
  → else call POST /api/auth/refresh-token (also cookie-based)
      → success: retry the original request once
      → failure: clear session store, redirect to login
```

The "queue behind an in-flight refresh" step matters: multiple 401s firing concurrently (e.g. several parallel queries) must trigger exactly one refresh call, not one per failed request.

## Endpoint function contract

```ts
// src/services/product/search-products.ts
export function searchProducts(
  params: SearchProductsParams,
): Promise<BackendPaginatedResult<SearchProductsItemResponse>> {
  return apiClient.get(`${BASE_PATH}/products`, { params }).then((res) => res.data);
}
```

Endpoint functions return only business data — never the Axios response object, never the still-enveloped shape. This is enforced by always routing through the shared response interceptor rather than each endpoint file re-implementing unwrapping. Endpoints whose response carries no meaningful `data` (login/logout/refresh/register, enable/disable a channel, mark-as-read) return `Promise<void>`.

Full rationale for this shape (esp. the refresh-queueing behavior and the new `services/` layer) in [decisions/0005-api-layer-and-auth.md](../decisions/0005-api-layer-and-auth.md) and [decisions/0012-backend-service-client-layer.md](../decisions/0012-backend-service-client-layer.md).
