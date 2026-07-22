# Reference: Search (Elasticsearch)

**Scope:** the reusable Search read-model infrastructure introduced by the Product Search feature, and the Product-specific implementation built on top of it. Read this before touching `BuildingBlock.Search`, `Product.Persistence.Elasticsearch`, or any future service's search integration.

## Responsibility

Elasticsearch is a **read model, never a source of truth**. PostgreSQL remains authoritative for every service. Elasticsearch exists only to serve search/list queries efficiently — full-text keyword search, faceted filtering, sorting — that Postgres `ILIKE` scans don't do well at scale.

**Hard rules:**
- No command handler ever writes to Elasticsearch directly or synchronously.
- All writes to Elasticsearch originate from PostgreSQL, flow through the existing Outbox → Kafka pipeline, and land asynchronously via a dedicated consumer.
- Elasticsearch is eventually consistent with Postgres, never the reverse.

## Architecture: reusable vs. Product-specific

Search is **not** a new microservice. Each service that adopts Search owns its own index and its own sync pipeline, reusing only the technology-agnostic 20% via `BuildingBlock.Search`. Product Search (the only full implementation today) demonstrates the pattern; Customer Search and Order Search would each repeat it independently, inside their own service.

```
BuildingBlock.Search (reusable — client, generic indexer, config, retry policy)
  ├── used by → Product.Persistence.Elasticsearch (Product-specific: document, mapping, repository, indexer wrapper)
  ├── used by → (future) Customer.Persistence.Elasticsearch
  └── used by → (future) Order.Persistence.Elasticsearch
```

### `BuildingBlock.Search` (reusable)

Zero project references (root-level BuildingBlock, like `SharedKernel`), one package: `Elastic.Clients.Elasticsearch`.

- `Configuration/ElasticsearchOptions.cs` — `Url`, `MaxRetries`, `RequestTimeoutSeconds`.
- `Abstractions/IElasticsearchIndexer.cs` — generic `IElasticsearchIndexer<TDocument>`: `EnsureIndexAsync`, `RecreateIndexAsync`, `IndexAsync`, `DeleteAsync`, `BulkIndexAsync`. This is the **only** reusable component allowed to write to Elasticsearch — querying is deliberately not part of this interface, since query DSL is too domain-specific to generalize.
- `Indexing/ElasticsearchIndexer.cs` — the implementation, wraps a singleton `ElasticsearchClient`.
- `DependencyInjection/ServiceCollectionExtensions.cs` — `AddElasticsearchClient(configuration)`: binds `ElasticsearchOptions` from the `"Elasticsearch"` config section, registers `ElasticsearchClient` as a singleton (thread-safe/stateless, same lifetime rationale as `Persistence.Mongo`'s `IMongoClient`) with `MaxRetries`/`RequestTimeout` wired from options (the "retry policies" extension point — no Polly needed, the Elastic client's built-in transport retry covers it), and registers `IElasticsearchIndexer<>` as an open-generic singleton.

**Deliberately not included yet** (per the task's "prepare extension points, don't implement unnecessary monitoring" guidance):
- No health check package wired — the extension point is `BuildingBlock.Web/HealthChecks/HealthCheckExtensions.cs`'s `AddHealthCheckServices()` chain; a future `AddElasticsearchHealthCheck()` would slot in there.
- No OpenTelemetry/metrics/tracing — `ElasticsearchClientSettings` supports `OnRequestCompleted`/instrumentation hooks natively when needed.
- No generic query/search abstraction — every service's search criteria and result shape differ enough that a shared interface would be premature; each service's Search Repository talks to `ElasticsearchClient` directly.

### Product-specific (Product.Application + Product.Persistence.Elasticsearch)

Everything below lives inside Product's own projects, never in a BuildingBlock — per the task's explicit instruction that `ProductSearchDocument`/`ProductProjectionBuilder`/`ProductSearchRepository` remain Product-specific.

**`Product.Application/Abstractions/Search/`** (interfaces + document/criteria, mirrors `Abstractions/Repositories/`):
- `ProductSearchDocument.cs` — the read-model document. Deliberately not the `Product` aggregate: `ProductId`, `Code`, `Name`, `Slug`, `Thumbnail`, `DefaultPrice`, `DefaultVariationId`/`Sku`, `CategoryIds`/`CategoryNames`, `TagIds`/`TagNames`, `Status`, `UpdatedAt`. Optimized for query/display, not persistence — no full variation list, no metadata blobs.
  - `Status` is a deliberate stand-in: `Product` itself has no lifecycle status field, so the document uses the **Default Variation's** `Status` (Active/Inactive/Discontinued).
  - `Thumbnail` is the Default Variation's first image URL, if any.
- `ProductSearchCriteria.cs` — `Keyword`, `CategoryId`, `TagId`, `Status`, `SortBy`, `SortDescending`, `Page`, `PageSize`. Adding a new filter (price range, brand, attributes) later is just a new optional field here — no redesign of the repository or query pipeline.
- `IProductSearchRepository.cs` — **query-only**: `SearchAsync(ProductSearchCriteria, ct)`. No `Add`/`Update`/`Delete` — indexing is a completely separate interface.
- `IProductSearchIndexer.cs` — **write-only**: `EnsureIndexAsync`, `RecreateIndexAsync`, `IndexAsync`, `DeleteAsync`, `BulkIndexAsync`. Product's thin wrapper around `IElasticsearchIndexer<ProductSearchDocument>`, fixing the index name and mapping.

**`Product.Application/Features/Products/Search/ProductSearchProjectionBuilder.cs`** — the Projection Builder: **Integration Event → Search Document**. `BuildAsync(ProductEntity, ct)` (single) and `BuildManyAsync(IReadOnlyList<ProductEntity>, ct)` (batched, preloads all categories/tags once instead of N+1). This is the **only** place `ProductSearchDocument` is assembled — both the live sync path and the rebuild path call into it, so a future schema change touches exactly one class.

**`Product.Persistence.Elasticsearch`** (peer project to `Product.Persistence`, not nested inside it — mirrors how `Persistence`/`Persistence.Ef`/`Persistence.Mongo` are independent peers):
- `Search/ProductSearchIndexNames.cs` — the literal index name (`product-search`), the only place it's hardcoded.
- `Mappings/ProductSearchIndexMapping.cs` — the ES field mapping (keyword fields for ids/codes/status, text+keyword-subfield for `Name`, double for price, date for `UpdatedAt`).
- `Search/ProductSearchIndexer.cs` — `IProductSearchIndexer` impl, delegates to `IElasticsearchIndexer<ProductSearchDocument>` with the fixed index name/mapping.
- `Search/ProductSearchRepository.cs` — `IProductSearchRepository` impl. Builds a `bool` query directly against `ElasticsearchClient`: `must` multi-match on `Keyword` across `name`/`categoryNames`/`tagNames` when present, `filter` terms on `categoryIds`/`tagIds`/`status` when present, `sort` on `name.keyword`/`defaultPrice`/`updatedAt`, `from`/`size` for paging.
- `DependencyInjection.cs` — `AddElasticsearchPersistence(configuration)`: calls `BuildingBlock.Search`'s `AddElasticsearchClient`, then registers the Product-specific indexer/repository.

## Synchronization flow

Product Service is **both publisher and consumer** of its own integration events — it self-consumes via its own Outbox → Kafka → its own Kafka consumer, exactly like any other cross-service consumer in this codebase, just looping back to itself. This decouples the write path (Postgres) from the read-model update (Elasticsearch) without a synchronous dependency, and lets the sync retry/backoff independently via the existing Inbox mechanism.

```
CreateProduct / UpdateProduct / DeleteProduct / AddVariation / UpdateVariation / DeleteVariation
AssignProductCategory / RemoveProductCategory / AssignProductTag / RemoveProductTag
  (Command Handlers, Product.Application)
    ↓ IOutboxStore.EnqueueAsync — same transaction as the aggregate write, unchanged pattern
Outbox (Postgres) → OutboxRelayHostedService → Kafka
    ↓
Product.Infrastructure/Messaging/Consumers/*IntegrationEventConsumer
    (10 thin consumers, one per topic — deserialize, log, dispatch an internal event; no business logic)
    ↓ IInternalEventDispatcher.PublishAsync
OnProductSearchSyncRequiredEvent   (9 of the 10 consumers — every event except ProductDeleted)
    ↓ OnProductSearchSyncRequiredHandler:
       productRepo.GetByIdAsync → ProjectionBuilder.BuildAsync → IProductSearchIndexer.IndexAsync (upsert)
OnProductSearchRemovalRequiredEvent   (the ProductDeleted consumer only)
    ↓ OnProductSearchRemovalRequiredHandler: IProductSearchIndexer.DeleteAsync(productId)
```

**Why one handler rebuilds the whole document instead of applying partial updates:** every sync-triggering event (Created/Updated/VariationCreated/VariationUpdated/VariationDeleted/CategoryAssigned/CategoryRemoved/TagAssigned/TagRemoved) funnels into the same `OnProductSearchSyncRequiredEvent` → the handler reloads the current Product from Postgres and rebuilds the full document. This is simpler and strictly more correct than threading 9 different partial-update shapes through the indexer, and it means Postgres is always the source of truth for what gets indexed — the integration event is only a "something changed, go re-sync" signal, never a payload the index trusts directly.

**New integration events added for this feature:** `AssignProductCategory`/`RemoveProductCategory`/`AssignProductTag`/`RemoveProductTag` previously published no event at all. `ProductCategoryAssignedIntegrationEvent`, `ProductCategoryRemovedIntegrationEvent`, `ProductTagAssignedIntegrationEvent`, `ProductTagRemovedIntegrationEvent` (`BuildingBlock.Contract/Events/Product/`) were added purely to keep the Search index in sync — no other consumer needs them today.

**Product now has an Inbox table for the first time** (previously publish-only, see [inbox-outbox-runtime.md](inbox-outbox-runtime.md)) — self-consumption requires the same dedup/retry/dead-letter guarantees any other consumer gets. `AddInboxOutboxCleanupJobs(configuration)` replaces the old Outbox-only cleanup registration.

## Rebuild strategy

`POST /products/search/rebuild` (RequireAdmin) → `RebuildProductSearchIndexCommand` → `RebuildProductSearchIndexHandler`:

```
PostgreSQL → (paged, 200/batch, via IProductRepository.GetAllAsync) →
  ProjectionBuilder.BuildManyAsync → IProductSearchIndexer.BulkIndexAsync → Elasticsearch
```

`RecreateIndexAsync` runs once at the start (drop + recreate with the current mapping), then each batch is bulk-indexed. This reuses the **exact same** `ProductSearchProjectionBuilder` and `IProductSearchIndexer` the live sync path uses — proving the projection/indexing code is shared, not duplicated, between the event-driven and rebuild paths. This is the pattern any future service's rebuild endpoint should follow.

Runs synchronously in the command handler today (no background job) — appropriate for the current catalog scale. If catalogs grow large enough that this blocks a request for too long, the natural extension point is a Hangfire `IRecurringJob`/one-off job wrapping the same handler logic, not a redesign.

## Query flow

`GET /products` → `SearchProductsQuery` → `SearchProductsHandler` → `IProductSearchRepository.SearchAsync` → **Elasticsearch only**, never Postgres. This replaced the previous Postgres `ILIKE`-based `ListProductsQuery`/`ProductRepo.SearchAsync`, which were deleted as dead code once ES became the only Product-list path.

`GET /products/{productId}` (Product Detail) is untouched — still reads Postgres directly via `IProductRepository`. Only the *list/search* surface moved to Elasticsearch, per the task's explicit scope.

## Future extension points

- **Customer Search / Order Search**: repeat the Product pattern — a `{Service}.Persistence.Elasticsearch` project referencing `BuildingBlock.Search` + that service's own `Application` project, a document/criteria/repository/indexer/mapping quartet, a self-consuming (or cross-service-consuming, if the read model naturally lives in a different service) Kafka sync pipeline, and a rebuild command. Nothing in `BuildingBlock.Search` needs to change.
- **Kibana dashboards**: Kibana is already provisioned in `docker-compose.yml`/`.override.yml` pointing at the shared `elasticsearch` container — any index created here (`product-search`, and future `customer-search`/`order-search`) is visible in Kibana with zero additional plumbing.
- **Audit Analytics**: Audit Service could adopt the same `BuildingBlock.Search` indexer for an analytics-oriented index over `AuditLogEntry` data, independent of this feature.
- **Health checks / OpenTelemetry / metrics**: see "Deliberately not included yet" above — the extension points are documented, not implemented.
- **Blue/green reindexing via aliases**: today `RecreateIndexAsync` does a blocking drop+create. A zero-downtime rebuild (index-with-versioned-name + alias swap) is a natural upgrade to `IElasticsearchIndexer<TDocument>` if rebuild frequency/downtime ever becomes a real constraint — not implemented now since it's not yet needed.
