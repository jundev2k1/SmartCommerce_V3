# Backend: User Service

**Purpose:** What the User Service exposes and how `src/services/user` maps to it.

**Scope:** This service's contract only.

**Related documents:** [backend/README.md](../README.md), [backend/feature-mapping.md](../feature-mapping.md), [backend/coverage-report.md](../coverage-report.md), `src/services/user/`

**When to read:** Building User Management (Phase 4) or anywhere user profile data is needed.

**When to ignore:** Any work not touching user profiles.

---

## Purpose

User profile management — separate from Auth (which owns credentials/session); shares identity via `UserId`. Roles are cached from the Auth service, not owned here.

## Base path

`/api/user`.

## Auth requirements

Not explicitly stated per-endpoint beyond general platform auth; `CreateUser` requires `X-Correlation-Id` (sent globally, see [services/api-layer.md](../../services/api-layer.md)). Role-granting rules are documented in prose: "Root may grant Admin or User; Admin may only grant User" — enforced server-side.

## Endpoints

| Operation               | Method & Path                  | Client function                                 |
| ----------------------- | ------------------------------ | ----------------------------------------------- |
| Create user             | `POST /profiles`               | `createUser(request)`                           |
| Get user                | `GET /profiles/{userId}`       | `getUser(userId)`                               |
| Get current user detail | `GET /profiles/current/detail` | `getUserDetail()`                               |
| Update user             | `PUT /profiles/{userId}`       | `updateUser(userId, request)` — `Promise<void>` |

## Request/response DTOs

`CreateUserRequest`, `UpdateUserRequest`; `CreateUserResponse`, `GetUserResponse`, `GetUserDetailResponse` (includes `roles: string[] | null`, cached from Auth). See `src/services/user/`.

## Pagination style

N/A — no list endpoints in this contract.

## Error responses

Prose-only: 400 (invalid request / validation / duplicate email or username), 404 (not found), 500 (creation/retrieval/update failed). No formal error schema.

## Known limitations / TODOs

- [ ] **Route/prose mismatch on `GetUserDetail`.** The endpoint's actual path is `/profiles/current/detail` with zero path/query parameters (derives the current user from the auth session), but its Swagger _description_ text talks about a `userId` route parameter that doesn't exist on this route — almost certainly a stale copy-paste from `GetUserEndpoint`'s docs. The client function `getUserDetail()` takes no arguments, following the actual route, not the prose. Worth confirming with the backend team.
- [ ] `UserStatus` (1/2/3) is the one enum in this whole integration with a confirmed mapping (Active/Inactive/Suspended), stated directly in `GetUserDetail`'s prose — implemented as a proper `as const` union in `types/user-status.ts`, unlike every other enum encountered (see [coverage-report.md](../coverage-report.md)).
- [ ] `CreateUserRequest.roles`/`tempPassword` are typed optional based on prose phrasing ("Roles to grant..."), not an explicit "(required)" tag — worth confirming.
