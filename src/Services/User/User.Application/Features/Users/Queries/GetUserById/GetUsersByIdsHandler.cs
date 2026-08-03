using SmartEcommerce.User.Application.Abstractions.Services;
using SmartEcommerce.User.Application.Features.Users.Caching;

namespace SmartEcommerce.User.Application.Features.Users.Queries.GetUserById;

public sealed class GetUsersByIdsHandler(
    CachedUserProfileReader userProfileReader,
    IUserDisplayNameFormatter displayNameFormatter) : IQueryHandler<GetUsersByIdsQuery, IReadOnlyDictionary<Guid, UserLookupResult>>
{
    private const string GrpcDisplayLocale = "en";

    public async Task<IReadOnlyDictionary<Guid, UserLookupResult>> Handle(GetUsersByIdsQuery request, CancellationToken ct = default)
    {
        var profiles = await userProfileReader.GetManyAsync(request.UserIds, ct);

        return profiles.ToDictionary(
            kv => kv.Key,
            kv => new UserLookupResult(
                kv.Value.Id,
                kv.Value.Email,
                kv.Value.UserName,
                kv.Value.PhoneNumber,
                kv.Value.FirstName,
                kv.Value.MiddleName,
                kv.Value.LastName,
                displayNameFormatter.Format(kv.Value.FirstName, kv.Value.MiddleName, kv.Value.LastName, GrpcDisplayLocale),
                kv.Value.Status,
                kv.Value.Roles));
    }
}
