namespace SmartEcommerce.User.Application.Features.Users.Queries.GetUserById;

/// <summary>Backs the gRPC GetUser RPC - cache -&gt; DB -&gt; refresh cache -&gt; return, via CachedUserProfileReader.</summary>
public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserLookupResult?>;
