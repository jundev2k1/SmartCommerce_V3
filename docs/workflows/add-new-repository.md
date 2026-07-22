# Workflow: Add New Repository

**Read first:** [04-coding-rules.md](../04-coding-rules.md#repository--unit-of-work), [06-implementation-templates.md](../06-implementation-templates.md#repository-interface--implementation).

## Steps

1. **Check if you actually need a specific interface.** If your queries are covered by generic `IRepository<T>` (`GetByIdAsync`/`AddAsync`/`UpdateAsync`/`DeleteAsync`), inject `IRepository<{Entity}>` directly — no new file needed (User Service's accepted convention, see [services/user-service.md](../services/user-service.md#documented-divergence-from-auth-repository-style)).
2. **If you need more**, add `I{Entity}Repository : IRepository<{Entity}>` in `{Service}.Application/Abstractions/Repositories/` with only the extra method(s) you need.
3. **Implement** in `{Service}.Persistence/Repositories/{Entity}Repo.cs`, implementing both `IRepository<{Entity}>` and the specific interface.
4. **Do not manually register it in DI.** `AddScopedByInterface(typeof(IRepository<>), typeof({Service}DbContext))` (Scrutor scan, already wired in `{Service}.Persistence/DependencyInjection.cs`) picks up every `IRepository<T>` implementation in the assembly automatically.
5. **Repositories never call `SaveChanges`.** They stage changes only. The caller (handler, or a job service for multi-step work) owns `IUnitOfWork.SaveChangesAsync`/transaction boundaries — see [04-coding-rules.md#transaction-management](../04-coding-rules.md#transaction-management).

## Checklist

- [ ] Named `{Entity}Repo` (implementation) / `I{Entity}Repository` (interface) — matches the asymmetric naming convention
- [ ] No `SaveChanges`/`SaveChangesAsync` call inside the repository
- [ ] Not manually added to any `AddScoped<...>()` call — verify the Scrutor scan is picking it up instead
- [ ] If this repository needs a transaction spanning multiple repositories, that logic lives in the handler/job, not here
