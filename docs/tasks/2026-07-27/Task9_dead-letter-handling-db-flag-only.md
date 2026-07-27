# Task 9: Dead-letter handling is a DB status flag, not a real Kafka DLQ

**Status:** Open.

## Source

Full-system business-requirements audit, 2026-07-27. Requirement: Inventory reliability — verify "Dead Letter."

## Current state

`InboxMessageStatus.DeadLetter` marks a message dead in-table (part of the generic Inbox in `BuildingBlock.Persistence.Ef`). Per `InboxAttemptExecutor.cs:39-42`, dead-lettered messages are explicitly "not retried automatically" — there is no separate Kafka dead-letter topic, no automatic reprocessing path, and no operator-visible alert when a message lands in this state.

## Why this matters

A message that repeatedly fails today sits inert with no signal to anyone that it needs attention — it must be discovered by someone manually querying the Inbox table.

## Suggested acceptance criteria

- Dead-lettered messages are either replayable via a topic/admin action, or surfaced to an ops-visible dashboard/alert (even a simple periodic count-and-log is better than silence).
