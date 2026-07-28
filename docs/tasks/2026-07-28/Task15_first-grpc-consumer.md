# Task 15: First Real gRPC Consumer (Decision + Implementation)

**Status:** Not started (planning only) — **needs a product decision before implementation starts**
**Category:** gRPC

## Objective

Since no service currently consumes User via gRPC for reads at all, pick and build the first real consumer, so Task 13/14's new RPCs aren't shipped as unused infrastructure — following the existing client-registration convention (`BuildingBlock.Grpc.Client.GrpcClientExtensions.AddGrpcClient<T>()` + a thin per-service adapter), exactly as Order/Product already do for Inventory.

## Current state (grounded findings)

- **Confirmed: zero existing consumers of User's gRPC for reads, anywhere.** Order's `OrderOwner.CustomerName` (`Order.Domain/Entities/OrderOwner.cs:6-11`, doc comment quoted directly: "captured once at Create time, never resynced from the User service afterward, same convention `OrderItem.ProductName`/`UnitPrice` already follow") and Audit's `AuditTrailMetadata.Actor` (populated from request-time context via `HttpAuditMetadataProvider`, never a User lookup) both deliberately avoid this today by denormalizing/snapshotting instead. **Neither currently has a bug to fix** — this task is additive capability, and the team should explicitly decide it's worth introducing a new cross-service dependency before building it, rather than assuming the original request's premise (an existing inefficiency) is accurate.
- Registration convention to follow, confirmed identical across every existing gRPC client in the repo (Auth→User, Order→Inventory, Product→Inventory, User→Auth): a private `AddGrpcClients(IServiceCollection, IConfiguration)` method reads a URL from config (`Grpc:<ServiceName>:Url`, falling back to a hardcoded compose hostname), calls `services.AddGrpcClient<TGeneratedClient>(new Uri(url))` (the shared `BuildingBlock.Grpc.Client.GrpcClientExtensions` helper — 10MB message size + gzip decompression, no Polly/retry anywhere in the repo despite an unused `GrpcClientOptions.MaxRetries` field), then registers a thin adapter (`XxxClientService : IXxxClientService`) as Scoped.
- Concrete template to copy: `Order.Infrastructure/GrpcClients/InventoryClientService.cs:10-20`'s `GetAvailableStockBatchAsync(IReadOnlyCollection<Guid> ids)` — builds the repeated-field request, calls the generated client, maps `items` back into a `Dictionary<Guid, T>`.
- **Two plausible candidates, each with a real use case, neither yet decided:**
  - **Order**: replace (or offer as an alternative to) the free-text `CustomerName` snapshot with a live-resolved display name for *new* order-detail rendering — but this directly conflicts with the existing, deliberate "snapshot, never resynced" design documented in `OrderOwner.cs`'s own comment; changing that is a real product/architecture decision (should an order's displayed customer name reflect the person's *current* name, or the name as of order placement?), not something this task should decide unilaterally.
  - **Audit**: enrich `AuditTrailMetadata.Actor` (or an Audit *read/display* layer, not the stored value — audit records should stay immutable) with a live-resolved actor display name for the Audit Trail UI, without changing what's persisted. Lower risk than Order's case since it wouldn't touch an existing, deliberate design decision — likely the safer first consumer.

## Scope (once a candidate is chosen)

- Client side: `<Service>ClientService : IUserClientService` (or similarly named), implementing single `GetUserAsync(Guid)` and batch `GetUsersAsync(IReadOnlyCollection<Guid>)`, registered via the standard `AddGrpcClients` convention in that service's `Infrastructure/DependencyInjection.cs`.
- Consuming code: wherever the chosen service currently renders/needs a user's display name, call the batch method for list views (never loop single calls per row) and the single method for detail views.
- **Do not** change Order's persisted `CustomerName` snapshot semantics without an explicit, separate decision — if Order is chosen, scope this task to a *new*, additive "live customer info" capability (e.g. an optional enrichment on a detail view) rather than replacing the existing snapshot field.

## Dependencies

- **Depends on:** Task 14 (server RPCs must exist and work).
- **Blocks:** nothing else in this epic — this is the "prove it end-to-end" capstone of the gRPC work, not a prerequisite for anything else.

## Estimated complexity

Medium — mechanically small (client registration + adapter, following a proven pattern) but gated on a decision the team needs to make first; the Order option in particular has real design implications beyond this task's scope.

## Risks

- Building this without a clear consumer decision risks shipping speculative infrastructure that never gets exercised in production — confirm the candidate and its actual use case before writing code, per this repo's "don't design for hypothetical future requirements" convention.
- If Order is chosen and the implementation quietly starts resolving customer name live instead of from the snapshot, it silently breaks the documented, deliberate point-in-time-snapshot guarantee other code already depends on (`OrderItem.ProductName`/`UnitPrice` follow the same convention) — treat this as a hard boundary, not a detail to get "close enough."

## Completion checklist

- [ ] **Decision made and recorded**: which service is the first consumer, and what specific screen/use case it serves
- [ ] Client adapter implemented, registered via the standard `AddGrpcClients` convention
- [ ] Batch method used for any list-rendering call site (verified: no loop-of-single-calls introduced)
- [ ] If Order was chosen: confirmed the existing `CustomerName` snapshot's persisted semantics are unchanged; new capability is additive only
- [ ] End-to-end test: consumer successfully resolves a real user's display name via the new gRPC path, including the partial-not-found case
