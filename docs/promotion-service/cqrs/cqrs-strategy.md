# Promotion Service — CQRS Strategy

**Scope:** The future CQRS shape every Promotion feature follows once Phase 5 (CQRS Skeleton) begins. Documentation only — no handler, endpoint, or DTO exists yet. This does not introduce a new pattern; it commits Promotion Service to the platform's existing CQRS conventions, binding in [../../conventions/application-coding-conventions.md](../../conventions/application-coding-conventions.md) and [../../04-coding-rules.md](../../04-coding-rules.md), with a copy-paste starting point in [../../06-implementation-templates.md](../../06-implementation-templates.md).

## Per-feature shape

Every feature is a Feature-First folder under `Promotion.Application/Features/{Aggregate}/{Commands|Queries}/{FeatureName}/`, containing:

- **Commands** — `{Action}Command : ICommand<TResult>` (e.g. `CreatePromotionCommand`). One command = one write intent, no CRUD-generic command types.
- **Queries** — `{Action}Query : IQuery<TResult>` (e.g. `GetPromotionQuery`). Read-only, never mutates.
- **Validators** — `{Action}CommandValidator : AbstractValidator<TCommand>` (FluentValidation), colocated with the command/query it validates.
- **Handlers** — `{Action}Handler : ICommandHandler<TCommand, TResult>` / `IQueryHandler<TQuery, TResult>`. Thin orchestration only — persistence access goes through a Persistence Service (see [../persistence/persistence-strategy.md](../persistence/persistence-strategy.md)), not a raw `DbContext`.
- **DTOs** — `{Name}Dto`/`{Name}Result` per [04-coding-rules.md](../../04-coding-rules.md) naming, mapped from Domain via Mapster, never the Domain entity returned directly across the API boundary.
- **Contracts** — request/response shapes consumed by the API layer, kept separate from internal DTOs where the two diverge (matches the platform's existing Contract-vs-DTO split).
- **Persistence Services** — `I{Aggregate}ReadService`/`I{Aggregate}WriteService` the handler depends on; see [../persistence/persistence-strategy.md](../persistence/persistence-strategy.md) for the full split.
- **Repositories** — `I{Aggregate}Repository`, near-empty markers beyond `IRepository<T, TId>` unless the aggregate genuinely needs an extra query method (same precedent as Payment Service's `IPaymentRepository.GetByIdempotencyKeyAsync`).
- **API Endpoints** — one Carter endpoint per route under `Promotion.API/Endpoints/{Aggregate}/`, calling `ISender` only — no business logic in the endpoint itself.

## Scope discipline for Phase 5

Not every aggregate gets a full CQRS surface in the first CQRS pass. Follow Payment Service's precedent: implement Create + Get for the aggregates the architect's design marks as in-scope for that pass; every other aggregate keeps its EF mapping (from Phase 3) but has no repository/handler/endpoint until a later phase's design calls for it. This is a deliberate scope decision, not an oversight — record it the same way [../../services/payment-service.md](../../services/payment-service.md#persistence-readwrite-services) does.

## What this phase does not do

No business rule (discount calculation, eligibility evaluation, redemption logic) is inferred into a handler. A handler in this skeleton phase persists and returns, same as Payment's `CreatePayment`/`CreatePaymentIntent`/`CreateRefund` precedent — real workflow logic is explicitly out of scope until a future prompt requests it.
