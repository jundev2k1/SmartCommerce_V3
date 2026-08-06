# Infrastructure — TODO

**Phase:** 6 (Infrastructure Integration). Bootstrap phase (1) wires the cross-cutting reliability infra only (Outbox/Inbox, Kafka client, Idempotency, Redis cache, cleanup jobs) — see `DependencyInjection.cs`.

Still to add, in Phase 6, following [../../../docs/promotion-service/phases/phase-6-infrastructure-integration.md](../../../docs/promotion-service/phases/phase-6-infrastructure-integration.md):
- `Messaging/Consumers/{Event}IntegrationEventConsumer.cs` — one per event this service actually needs to react to.
- `BackgroundJobs/` — any real recurring job (only `AddInboxOutboxCleanupJobs` is wired today; `UseBackgroundJobsDashboard`/`UseBackgroundJobsScheduling` are intentionally not called yet, same as Payment Service's foundation phase).
- `GrpcClients/` — only if the architect's design has Promotion calling out to another service synchronously.

No consumer/job/client is added speculatively — each is added when the phase that actually needs it starts.
