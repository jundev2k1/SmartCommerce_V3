# Phase 6 — Infrastructure Integration

## Purpose

Wire the remaining cross-cutting infrastructure — Kafka Outbox/Inbox relay, integration event contracts, the Idempotency + DistributedLock framework, gRPC (only if the architect's design calls for it), background jobs — and the first real cross-service integration points (e.g. another service consuming a Promotion integration event), following existing platform conventions. No new infrastructure pattern invented here.

## Expected Output

- `BuildingBlock.Contract/Events/Promotion/**` integration event contracts, scoped to exactly what this phase's design calls for.
- `Promotion.Infrastructure` messaging consumers/producers.
- [../integration/](../integration/) populated with the resulting responsibility boundaries — mirrors the precedent set by [../../reference/payment-ownership-boundaries.md](../../reference/payment-ownership-boundaries.md).

## Build Verification

- Solution builds.
- Outbox relay / Inbox retry hosted services start cleanly under `docker-compose up`.
- A published integration event is observable on Kafka.

## Completion Criteria

Whichever integration points this phase's design specifies are live and documented in [../integration/](../integration/). No speculative event contracts beyond what's requested.

## Blocked Items

Changes to a *consuming* service (e.g. Order Service adding a new consumer for a Promotion event) are out of scope unless separately requested as their own task — matches the precedent set when Payment Service's foundation phase left `OrderPayment.RecordPayment` unwired.

## Dependencies

Phase 5 (CQRS Skeleton).
