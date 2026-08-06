# CQRS — TODO

**Phase:** 5 (CQRS Skeleton). Bootstrap phase (1) only creates this placeholder — no feature exists yet.

Follow [../../../../docs/promotion-service/cqrs/cqrs-strategy.md](../../../../docs/promotion-service/cqrs/cqrs-strategy.md): one Feature-First folder per aggregate action —

```
Features/{Aggregate}/Commands/{Action}/{Action}Command.cs
Features/{Aggregate}/Commands/{Action}/{Action}Handler.cs
Features/{Aggregate}/Commands/{Action}/{Action}Validator.cs
Features/{Aggregate}/Queries/{Action}/{Action}Query.cs
Features/{Aggregate}/Queries/{Action}/{Action}Handler.cs
```

Only the aggregates the architect's design marks in-scope for the Phase 5 pass get a full slice — matches Payment Service's "core lifecycle first" precedent. Handlers persist and return only; no business rule is inferred (`// TODO:` mark anything genuinely unknowable).

DTOs live in `Features/{Aggregate}/DTOs/`, mapped via Mapster — never the Domain entity returned directly across the API boundary.
