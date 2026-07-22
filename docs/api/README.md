# Backend API Integration

**Purpose:** Pointer to where backend contract integration actually lives now that it's happened.

**Scope:** Redirect only — real content is under [backend/](../backend/README.md).

**Related documents:** [backend/README.md](../backend/README.md), [backend/feature-mapping.md](../backend/feature-mapping.md), [backend/coverage-report.md](../backend/coverage-report.md), [decisions/0012-backend-service-client-layer.md](../decisions/0012-backend-service-client-layer.md)

**When to read:** Never, directly — this file only exists so an old link to `docs/api/README.md` still leads somewhere. Go straight to [backend/README.md](../backend/README.md).

**When to ignore:** Always, once you know to look in `docs/backend/` instead.

---

## Status: superseded

This placeholder (written in Phase 0, before any backend contract existed) is superseded by **[docs/backend/](../backend/README.md)**, populated in Phase 1.5 from the real Swagger/OpenAPI files for all 7 backend services (audit, auth, inventory, notification, order, product, user).

The corresponding client code lives in `src/services/<service>/` — see [decisions/0012-backend-service-client-layer.md](../decisions/0012-backend-service-client-layer.md) for why that layer exists and how it's organized.

## What's still open

Per [backend/coverage-report.md](../backend/coverage-report.md): a few endpoints future modules will need don't exist yet in the current contract (order list, warehouse list, inventory list, campaign activation) — check that report before assuming a module can be fully built. Phase 12 (Backend Integration Pass) remains the point at which any _mock_ data still in use gets replaced with live calls against these now-real endpoints.
