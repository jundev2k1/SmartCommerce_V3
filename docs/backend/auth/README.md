# Backend: Auth Service

**Purpose:** What the Auth Service exposes and how `src/services/auth` maps to it.

**Scope:** This service's contract only.

**Related documents:** [backend/README.md](../README.md), [backend/feature-mapping.md](../feature-mapping.md), [backend/coverage-report.md](../coverage-report.md), [decisions/0005-api-layer-and-auth.md](../../decisions/0005-api-layer-and-auth.md), `src/services/auth/`

**When to read:** Building the Auth module (Phase 2).

**When to ignore:** Any work not touching login/session.

---

## Purpose

Authentication and authorization — login, logout, refresh, registration. Session state lives entirely in HTTP-only cookies; no tokens ever appear in a response body.

## Base path

`/api/auth` (separate dev port, `localhost:5100`, from the gateway-fronted services — irrelevant to the frontend, which only ever talks to `/api/auth/*` relative to the gateway origin).

## Auth requirements

- `/login`, `/register`: unauthenticated.
- `/logout`: requires a valid access token (cookie or `Authorization` header, per Swagger — but see the ambiguity below).
- `/refresh-token`: requires the `RefreshToken` cookie.

## Endpoints

| Operation     | Method & Path         | Client function                                                                              |
| ------------- | --------------------- | -------------------------------------------------------------------------------------------- |
| Login         | `POST /login`         | `login(request)` — `Promise<void>`                                                           |
| Logout        | `POST /logout`        | `logout()` — `Promise<void>`                                                                 |
| Refresh token | `POST /refresh-token` | `refreshToken()` — `Promise<void>` (also called directly by the shared Axios 401-retry flow) |
| Register      | `POST /register`      | `register(request)` — `Promise<void>`                                                        |

All four return `ObjectApiResponse` with `data: null` — there is nothing to unwrap beyond success/failure.

## Request/response DTOs

`LoginRequest { email, password }`, `RegisterRequest { email, password, firstName, lastName, phoneNumber }`. No response DTOs — see above.

## Pagination style

N/A — no list endpoints.

## Error responses

Documented only in prose: `login` → 401 invalid credentials, 400 invalid params; `refresh-token` → 400 missing cookie, 401 invalid/expired, 404 user not found; `register` → 400 invalid/email exists, 500 registration failed. No formal schema for any of these — normalized to `ApiError` uniformly per [services/api-layer.md](../../services/api-layer.md).

## Common envelope

Same `ApiResponseEnvelope<T>` as every other service (see [services/api-layer.md](../../services/api-layer.md)).

## Known limitations / TODOs

- [ ] **Bearer-vs-cookie ambiguity.** The Swagger document declares a `Bearer` `securitySchemes` entry and a top-level `security: [{ Bearer: [] }]` requirement, but every endpoint's own prose explicitly says auth is cookie-based ("No tokens in response body"). Treated as vestigial backend-framework boilerplate — the frontend implements cookie auth only, per ADR 0005. Flagged in [coverage-report.md](../coverage-report.md) as an ambiguous/contradictory contract detail worth raising with the backend team.
- [ ] `register` and (per the User service) `POST /profiles` are the only two endpoints that formally require `X-Correlation-Id`; the frontend sends it on every request for consistency (see [services/api-layer.md](../../services/api-layer.md)) rather than special-casing these two.
- [ ] No documented response body shape for any error case beyond the generic envelope with `success: false`.
