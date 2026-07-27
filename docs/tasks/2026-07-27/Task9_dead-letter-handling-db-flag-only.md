# Task 9: Dead-letter handling is a DB status flag, not a real Kafka DLQ

**Status:** Resolved 2026-07-27 — periodic count-and-log monitor added (see below). A real
replay-from-topic DLQ was judged out of scope for this pass; see "What wasn't done."

## Source

Full-system business-requirements audit, 2026-07-27. Requirement: Inventory reliability — verify "Dead Letter."

## Current state

`InboxMessageStatus.DeadLetter` marks a message dead in-table (part of the generic Inbox in `BuildingBlock.Persistence.Ef`). Per `InboxAttemptExecutor.cs:39-42`, dead-lettered messages are explicitly "not retried automatically" — there is no separate Kafka dead-letter topic, no automatic reprocessing path, and no operator-visible alert when a message lands in this state.

## Why this matters

A message that repeatedly fails today sits inert with no signal to anyone that it needs attention — it must be discovered by someone manually querying the Inbox table.

## Suggested acceptance criteria

- Dead-lettered messages are either replayable via a topic/admin action, or surfaced to an ops-visible dashboard/alert (even a simple periodic count-and-log is better than silence).

## What was done

Added `IInboxStore.GetDeadLetterSummaryAsync` (both the `BuildingBlock.Persistence.Inbox` and
`BuildingBlock.Application.Abstractions.Outbox` contracts), returning counts grouped by
`(ConsumerName, Topic)` with the oldest `DeadLetteredAt` timestamp per group. Implemented in
`EfInboxStore`/`MongoInboxStore` and wired through all 7 services' `Reliability/Inbox/InboxStore.cs`
adapters.

A new recurring job, `InboxDeadLetterMonitorJob` (`BuildingBlock.Infrastructure/BackgroundJobs/Monitoring/`),
runs every 15 minutes (configurable via `Jobs:InboxDeadLetterMonitor`, same options shape as
`InboxCleanupJob`/`OutboxCleanupJob`) and logs a `Warning` per dead-lettered group - consumer, topic,
count, and how long it's been stuck. It piggybacks on the existing `AddInboxOutboxCleanupJobs`
opt-in call already present in every service's DI, so no per-service wiring was needed beyond the
one shared registration point (`CleanupJobsExtensions.cs`).

## What wasn't done

A real Kafka DLQ topic (publish-to-topic + admin replay tooling) was not built. The suggested
acceptance criteria explicitly treats "even a simple periodic count-and-log" as sufficient, and the
existing `InboxRetryHostedService`/manual-requery path already allows a dead-lettered row to be
un-stuck by hand once someone is alerted to it - a full replay pipeline would be new scope, not a
gap-closing fix, and wasn't pursued without confirming it's wanted.
