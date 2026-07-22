# 0005 — Single Axios Client, Cookie Auth, Unified Envelope, Queued Refresh

**Status:** Accepted

**Date:** 2026-07-20

## Context

Every feature needs to call the backend. Without a single shared client, each feature would reinvent auth handling, error unwrapping, and 401/refresh logic — with a high chance of subtle inconsistencies (e.g. one feature refreshing tokens differently than another, or a race condition where concurrent 401s trigger multiple simultaneous refresh calls).

## Decision

One Axios instance in `shared/lib/api/client.ts`. Auth is HTTP-only-cookie based (`withCredentials: true`, no client-stored token). A response interceptor unwraps a unified response envelope into plain business data or a typed `ApiError`. A 401 triggers a queued refresh-then-retry flow: concurrent 401s share a single in-flight refresh call rather than each firing its own.

## Rationale

- HTTP-only cookies mean the frontend never handles raw tokens — removes an entire class of XSS-token-theft risk from the frontend's responsibility.
- A unified envelope-unwrapping interceptor means every `.service.ts` function returns clean business data — features never deal with transport-level shape.
- Queuing 401s behind one refresh call is necessary because a typical page fires several parallel queries; without queuing, an expired session would trigger a refresh-call stampede.

## Alternatives considered

- **Per-feature Axios instances/interceptors**: rejected — guaranteed drift in auth/error handling across 15+ features over a multi-year project.
- **Client-stored JWT (localStorage/memory)**: rejected — HTTP-only cookies were specified as the auth mechanism; also avoids XSS token-exfiltration risk that client-stored tokens carry.
- **Let each failed request independently trigger its own refresh call**: rejected — causes redundant refresh calls and potential race conditions where a request retries against a token that's mid-refresh.

## Consequences

- The exact envelope shape and refresh endpoint contract are placeholders until the real OpenAPI spec lands — see [api/README.md](../api/README.md). This decision fixes the _strategy_, not the literal field names.
- Any new feature's `.service.ts` automatically gets unwrapping, cookie auth, and refresh handling for free by using the shared client — no per-feature opt-in needed.

## Related documents

[services/api-layer.md](../services/api-layer.md), [services/error-handling.md](../services/error-handling.md), [api/README.md](../api/README.md)
