namespace SmartEcommerce.User.Application.Features.Users.Queries.GetUserById;

/// <summary>
/// Backs the gRPC GetUsers batch RPC. Never falls back to a loop of single lookups - exactly
/// one CachedUserProfileReader.GetManyAsync call, which itself does at most one
/// IUserProfileReadService.GetByIdsAsync round trip for whatever wasn't already cached.
/// </summary>
public sealed record GetUsersByIdsQuery(IReadOnlyList<Guid> UserIds) : IQuery<IReadOnlyDictionary<Guid, UserLookupResult>>;
