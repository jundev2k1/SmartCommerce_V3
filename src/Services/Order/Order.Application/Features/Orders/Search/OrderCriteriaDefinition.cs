using BuildingBlock.Criteria.Definition;
using BuildingBlock.Criteria.Strategies;

namespace Order.Application.Features.Orders.Search;

/// <summary>Admin search whitelist for <see cref="OrderEntity"/>. Built once (static singleton) - no per-request reflection scan.</summary>
public static class OrderCriteriaDefinition
{
    public static readonly CriteriaDefinition<OrderEntity> Instance = CriteriaDefinition<OrderEntity>.Create()
        // Exact match only - a GUID has no meaningful prefix/suffix/contains match, so Eq/Ne/In/NotIn (Guid's
        // default operator set) is intentionally not narrowed further.
        .Field(x => x.Id).Guid()
        .Field(x => x.Owner.CustomerName).String().Sortable().KeywordSearchable().IgnoreCase()
        .Field(x => x.Owner.CustomerPhone, name: "phone").UsePhoneSearch(x => x.Owner.CustomerPhoneSearch, x => x.Owner.CustomerPhoneReverse)
        .Field(x => x.Status).Enum().Sortable()
        .Field(x => x.CreatedAt).DateTime().Sortable()
        .Build();
}
