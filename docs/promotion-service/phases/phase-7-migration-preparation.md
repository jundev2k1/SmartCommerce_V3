# Phase 7 — Migration Preparation

## Purpose

Final production-readiness pass before real business-logic implementation begins: migration review, seed data, finalized docker/env/gateway wiring, completed documentation, and a closed-out readiness checklist.

## Expected Output

- `docs/services/promotion-service.md` — the standard single-page service doc, following the shape of [../../services/payment-service.md](../../services/payment-service.md)/[../../services/order-service.md](../../services/order-service.md), linked from [../../README.md](../../README.md).
- [../planning/PROGRESS.md](../planning/PROGRESS.md) updated to 7/7, all phase checkboxes closed.
- Readiness checklist reviewed against [../../workflows/new-service-scaffold.md](../../workflows/new-service-scaffold.md).

## Build Verification

- Full solution build.
- `docker-compose up` clean start.
- Migration re-verified against a fresh database.

## Completion Criteria

PromotionService is structurally complete and documented — ready for the next stage (real business-rule implementation: evaluation engine, discount calculation, redemption workflows), which is explicitly out of scope for this roadmap and requires its own future prompt(s).

## Blocked Items

None expected. Any gap found during this pass becomes a dated [../../tasks/](../../tasks/) entry rather than being fixed inline, per the "never infer missing business logic" rule.

## Dependencies

Phase 6 (Infrastructure Integration).
