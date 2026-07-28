using User.Application.Abstractions.Services;
using User.Application.Features.Users.Caching;

namespace User.Application.Features.Users.Queries.GetUserById;

public sealed class GetUserByIdHandler(
    CachedUserProfileReader userProfileReader,
    IUserDisplayNameFormatter displayNameFormatter) : IQueryHandler<GetUserByIdQuery, UserLookupResult?>
{
    // gRPC callers are services, not an authenticated HTTP request with its own Accept-Language -
    // fixed default locale, matching the search index's same simplification. See
    // docs/tasks/2026-07-28/Task13_grpc-proto-getuser-getusers.md.
    private const string GrpcDisplayLocale = "en";

    public async Task<UserLookupResult?> Handle(GetUserByIdQuery request, CancellationToken ct = default)
    {
        var profile = await userProfileReader.GetAsync(request.UserId, ct);
        if (profile is null)
            return null;

        return new UserLookupResult(
            profile.Id,
            profile.Email,
            profile.UserName,
            profile.PhoneNumber,
            profile.FirstName,
            profile.MiddleName,
            profile.LastName,
            displayNameFormatter.Format(profile.FirstName, profile.MiddleName, profile.LastName, GrpcDisplayLocale),
            profile.Status,
            profile.Roles);
    }
}
