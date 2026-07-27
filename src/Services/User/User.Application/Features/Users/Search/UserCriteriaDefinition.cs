using BuildingBlock.Criteria.Definition;
using BuildingBlock.Criteria.Enums;
using BuildingBlock.Criteria.Strategies;

namespace User.Application.Features.Users.Search;

/// <summary>Admin search whitelist for <see cref="UserProfile"/>. Built once (static singleton) - no per-request reflection scan.</summary>
public static class UserCriteriaDefinition
{
    public static readonly CriteriaDefinition<UserProfile> Instance = CriteriaDefinition<UserProfile>.Create()
        .Field(x => x.UserName).String().Sortable().KeywordSearchable().IgnoreCase()
        .Field(x => x.Email).String().Sortable().KeywordSearchable().IgnoreCase()
        .Field(x => x.FirstName).String().KeywordSearchable()
        .Field(x => x.LastName).String().KeywordSearchable()
        .Field(x => x.Status).Enum().Sortable()
        .Field(x => x.PhoneNumber, name: "phone").UsePhoneSearch(x => x.PhoneSearch, x => x.PhoneReverse)
        .Field(x => x.CreatedAt).DateTime().Sortable()
        // Not Sortable() - Roles is multi-valued, sorting by an array column isn't a coherent request.
        .Field(x => x.Roles, name: "role").UseStrategy(
            new StringCollectionContainsStrategy<UserProfile>(x => x.Roles), CriteriaOperator.Eq, CriteriaOperator.Ne)
        .Build();
}
