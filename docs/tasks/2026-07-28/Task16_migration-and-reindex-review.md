# Task 16: Migration/Reindex Review

**Status:** Not started (planning only)
**Category:** Infrastructure

## Objective

Confirm the whole epic can be rolled out against existing production data with no loss and no extended downtime — the required "review migration impact... ensure existing users are migrated safely... determine whether a full reindex job is required" checkpoint from the original request, done as a single, explicit review rather than assumed piecemeal across the other tasks.

## Current state (grounded findings)

- **`MiddleName` (Task 1) is additive and safe by construction**: new column, `NOT NULL DEFAULT ''`, no backfill needed beyond the default — matches the shape of the two most recent existing migrations (`20260721044607_AddUserPhoneSearchFields.cs`, which *did* need a raw-SQL backfill for its two columns, and `20260724060832_AddUserProfileRoles.cs`, which used a default-value-only approach with no backfill, the closer analog for `MiddleName`). No data loss risk.
- **Elasticsearch requires a full reindex, not an incremental one, after the mapping/document-shape changes in Tasks 7/8**: Product's own history confirms this exact lesson the hard way — `docs/tasks/2026-07-27/Task15_product-search-missing-variation-name.md`'s "What wasn't done" section states plainly: "existing indexed documents won't retroactively gain [a new field] otherwise" after a mapping change; whoever deploys must trigger the rebuild endpoint. User's first-ever index build is even more fundamental (there's no pre-existing index at all) — the sequence must be: deploy Task 6-9's code → run `RebuildUserSearchIndex` once against production data → **then** cut `SearchUsers` over (Task 10) to read from it. Cutting over before the rebuild runs would serve an empty/incomplete index to real admin users.
- **Product's rebuild is a blocking drop+recreate** (`ElasticsearchIndexer.RecreateIndexAsync`, confirmed no blue/green alias swap exists anywhere in this codebase) — during a production rebuild, `SearchUsers` (if already cut over) would see a briefly empty or partial index. For User's *first* rebuild this doesn't matter (nothing has cut over yet); for any *future* rebuild after User is live in production, this is a real, documented, accepted limitation inherited from Product (not something this task should try to fix — matching Product's current scope, not scope-creeping into blue/green indexing, which `docs/reference/search.md` itself flags as a deliberately deferred future extension).
- **Redis cache (Task 11/12) requires no migration at all** — it's populated lazily on first read; an empty cache at deploy time is the expected, normal state (a burst of cache misses immediately after deploy, no different from a cold cache after any Redis restart).
- **gRPC (Task 13/14) is purely additive to the proto** — no migration concern, only a coordinated-deploy concern (Task 13's own risk section covers this).

## Scope

- Write a short, explicit rollout runbook (this task's deliverable) covering the exact sequence: (1) deploy Task 1-5's code (name model + locale/display-name, safe to deploy standalone, fully backward compatible); (2) deploy Task 6-9's code (ES scaffolding + config, index not yet serving traffic); (3) run `RebuildUserSearchIndex` once against production; (4) verify document count/spot-check a few real records; (5) deploy Task 10 (cutover) only after step 4 passes; (6) deploy Task 11-15 (cache + gRPC) independently, any time, since they have no ordering dependency on the ES work.
- Confirm (don't just assume) that every existing user row will produce a valid `UserSearchDocument` post-migration — in particular, rows seeded before `MiddleName` existed default to `""`, which Task 7's `SearchName` generation must handle gracefully (empty middle name → no extra token, not a literal `"null"` or double-space artifact).
- Explicit rollback plan per the architecture doc's already-stated positions (Task 1 additive/reversible; ES path stays behind the old Postgres search until Task 10's cutover is verified; cache removal is a one-line DI change; gRPC additive) — this task's job is to confirm those individual rollback stories still hold when the pieces are deployed together, not to invent new ones.

## Dependencies

- **Depends on:** effectively all other tasks (this is a review/gate, not a code-writing task) — sequence it last, immediately before or as part of the production rollout, not as a design-time exercise disconnected from the actual implementation.
- **Blocks:** nothing in the task list itself, but functionally gates the go-live decision.

## Estimated complexity

Small (as a document/checklist) — the complexity is in verifying the other tasks' claims hold up together, not in writing new code.

## Risks

- The single biggest risk this task exists to catch: someone deploys Task 10 (ES cutover) before ever running Task 9's rebuild endpoint against production — resulting in `SearchUsers` returning empty results for real admin users on day one. Make the rebuild-then-cutover ordering an explicit, checked gate, not an assumption.
- If the team decides to keep the Postgres search path around indefinitely (Task 10's dual-path option) rather than fully cutting over, this task's rollback story simplifies (just flip back), but the ongoing double-maintenance cost should be named explicitly as an accepted trade-off, not a silent default.

## Completion checklist

- [ ] Rollout runbook written and reviewed by the team (sequence above, adjusted for actual task completion order)
- [ ] Confirmed: pre-`MiddleName` rows produce clean, artifact-free `SearchName`/`DisplayName` values
- [ ] Confirmed: `RebuildUserSearchIndex` run and verified against production-shaped data **before** Task 10's cutover ships
- [ ] Rollback story re-confirmed for each phase once deployed together, not just per-task in isolation
