# Phase 5 — CQRS Skeleton

## Purpose

Build the CQRS skeleton per [../cqrs/cqrs-strategy.md](../cqrs/cqrs-strategy.md) — Commands, Queries, Validators, Handlers, DTOs, Contracts, Persistence Services, Repositories, API Endpoints — for whichever aggregates the architect's design marks as in-scope for this pass. Mirrors Payment Service's precedent: a proof-of-concept slice (Create + Get) for the core aggregates first, not every aggregate at once, unless the design explicitly says otherwise.

## Expected Output

- Feature-First folders under `Promotion.Application/Features/**` per [../../conventions/application-coding-conventions.md](../../conventions/application-coding-conventions.md).
- Repository + Read/Write persistence-service pairs under `Promotion.Application/Abstractions/Persistence/**` and `Promotion.Persistence/**`, per [../persistence/persistence-strategy.md](../persistence/persistence-strategy.md).
- Carter endpoints under `Promotion.API/Endpoints/**`.

## Build Verification

- Full solution builds.
- Endpoints are reachable under `docker-compose up`; Swagger reflects the new routes.

## Completion Criteria

- Every in-scope aggregate has, at minimum, Create + Get wired end-to-end from API to Persistence.
- FluentValidation validators wired per command/query.
- No business workflow logic beyond what's explicitly requested — persist and return, same as Payment's `CreatePayment`/`CreatePaymentIntent`/`CreateRefund` precedent. Anything genuinely unknowable is `TODO`-marked, never inferred.

## Blocked Items

Real promotion-evaluation/discount-calculation business rules are explicitly out of scope for this phase — they require their own future prompt after Phase 7 closes.

## Dependencies

Phase 3 (Persistence); Phase 4 (Search Integration) if any query in this pass routes through search.
