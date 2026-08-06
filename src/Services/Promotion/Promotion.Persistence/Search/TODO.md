# Search — TODO

**Phase:** 4 (Search Integration). Bootstrap phase (1) only creates this placeholder — no index/document/indexer exists yet.

Follow [../../../../docs/promotion-service/search/search-strategy.md](../../../../docs/promotion-service/search/search-strategy.md), mirroring `User.Persistence/Contexts/Users/Search/`'s shape once an aggregate exists to search:

```
Contexts/{Aggregate}/Search/PromotionSearchIndexNames.cs
Contexts/{Aggregate}/Search/Mapping/PromotionSearchIndexMapping.cs
Contexts/{Aggregate}/Search/Indexers/PromotionSearchIndexer.cs
Contexts/{Aggregate}/Search/Repositories/PromotionSearchRepository.cs
```

This top-level `Search/` folder is a Phase 1 placeholder only — the real implementation nests under `Contexts/{Aggregate}/Search/` once Phase 2 supplies an aggregate to index, matching every other per-aggregate folder in this project.
