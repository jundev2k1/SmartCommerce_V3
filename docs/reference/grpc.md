# Reference: gRPC

**Scope:** the gRPC client/server building blocks (`BuildingBlock.Grpc`, `BuildingBlock.Contract`). Condensed and pruned from the former `building-blocks/GRPC.md`, which self-flagged large sections (streaming, retry policy, service-mesh discovery) as unconfirmed against the actual current implementation — those sections are **not** carried forward here; verify against source before relying on anything beyond what's below. Two call chains today: Auth → User `CreateUserProfile`, and Order → Inventory (`GetProductStock` read-only check in `CreateOrderHandler`, `DeductStock`/`RestockStock` in the CreateOrder saga — see [reference/create-order-saga.md](create-order-saga.md) and [services/inventory-service.md](../services/inventory-service.md#grpc-inventorygrpcservice)).

## Contract-first

`.proto` files live in `BuildingBlock.Contract/Protos/` (currently `user.proto`), compiled with `GrpcServices="Both"` — generates both client and server stubs into `BuildingBlock.Contract.Protos.{X}` namespace. Add a new RPC here first, then implement client/server usage.

## Server side

```csharp
// {Service}.Infrastructure or API DependencyInjection
services.AddGrpcServer();   // BuildingBlock.Grpc.Server — wires LoggingInterceptor + ErrorHandlingInterceptor + health check
// {Service}.API/ApplicationPipeline.cs
app.MapGrpcServices();      // sets health status to Serving
app.MapGrpcService<{X}GrpcServiceImpl>();
```
Implement the generated `{X}GrpcServiceBase`, e.g. `User.API/GrpcServices/UserGrpcServiceImpl.cs` — keep it a thin adapter: parse request, dispatch a Command via `ISender`, no business logic.

## Client side

```csharp
services.AddGrpcClient<{X}GrpcService.{X}GrpcServiceClient>(new Uri(url));
```
10MB max message size + gzip decompression by default (`BuildingBlock.Grpc/Client/GrpcClientExtensions.cs`). Example: `Auth.Infrastructure/GrpcClients/UserProfileServiceClient.cs` wraps the generated client behind a service-specific interface (`IUserProfileService`) so the Application layer never touches gRPC types directly.

## Interceptors (server-side, applied automatically by `AddGrpcServer()`)

- `ErrorHandlingInterceptor` — maps `ArgumentNullException`→`InvalidArgument`, `InvalidOperationException`→`FailedPrecondition`, `UnauthorizedAccessException`→`Unauthenticated`, else→`Internal`, all as `RpcException`.
- `LoggingInterceptor` — logs method/peer/duration/status.

## When to use gRPC vs an integration event

gRPC: synchronous, same-transaction-adjacent need for a response (e.g. "create this profile and tell me if it worked"). Integration event: fire-and-forget notification another service should eventually react to. Auth's registration flow uses gRPC because it needs to know profile creation succeeded before completing registration — see [services/auth-service.md](../services/auth-service.md).
