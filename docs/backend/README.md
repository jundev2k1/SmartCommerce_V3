# Backend Contracts

**Purpose:** Entry point for everything the frontend knows about the real backend, integrated in Phase 1.5 from 7 provided OpenAPI/Swagger files. Future feature work should never need to reopen Swagger — everything needed is here or in `src/services/<service>/`.

**Scope:** Backend contract summaries and frontend↔backend mapping. Architectural rationale for the `src/services/` layer itself is in [decisions/0012-backend-service-client-layer.md](../decisions/0012-backend-service-client-layer.md), not here. Transport-layer mechanics (envelope, auth, correlation ID) are in [services/api-layer.md](../services/api-layer.md).

**Related documents:** [decisions/0012-backend-service-client-layer.md](../decisions/0012-backend-service-client-layer.md), [services/api-layer.md](../services/api-layer.md), [roadmap.md](../roadmap.md)

**When to read:** Before implementing any feature that calls a backend endpoint — check the relevant service doc and [feature-mapping.md](./feature-mapping.md) first.

**When to ignore:** Pure frontend-infrastructure work (theming, layout, forms wiring) with no API interaction.

---

## Directory

| Service      | Endpoints | Doc                                                |
| ------------ | --------- | -------------------------------------------------- |
| Audit        | 2         | [audit/README.md](./audit/README.md)               |
| Auth         | 4         | [auth/README.md](./auth/README.md)                 |
| User         | 4         | [user/README.md](./user/README.md)                 |
| Order        | 3         | [order/README.md](./order/README.md)               |
| Inventory    | 8         | [inventory/README.md](./inventory/README.md)       |
| Product      | 25        | [product/README.md](./product/README.md)           |
| Notification | 23        | [notification/README.md](./notification/README.md) |

**69 endpoints total**, each with a matching client function in `src/services/<service>/`. See [feature-mapping.md](./feature-mapping.md) for which frontend module consumes which service, and [coverage-report.md](./coverage-report.md) for the full endpoint checklist plus known gaps/ambiguities discovered while integrating these contracts.

## How this differs from the rest of `/docs`

Everything under `docs/backend/` describes _the backend as it actually is_ (per the Swagger provided in Phase 1.5), not frontend architecture decisions. When the backend contract changes (new endpoint, changed DTO), update the relevant `docs/backend/<service>/README.md` and the corresponding `src/services/<service>/` files together — this directory is meant to go stale exactly when the backend does, and get refreshed the same way it was built: from a new Swagger export, not by guessing.
