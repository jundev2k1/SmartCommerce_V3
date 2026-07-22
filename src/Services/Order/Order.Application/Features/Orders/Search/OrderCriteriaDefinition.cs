using BuildingBlock.Criteria.Definition;
using BuildingBlock.Criteria.Strategies;

namespace Order.Application.Features.Orders.Search;

/// <summary>Admin search whitelist for <see cref="OrderEntity"/>. Built once (static singleton) - no per-request reflection scan.</summary>
public static class OrderCriteriaDefinition
{
    public static readonly CriteriaDefinition<OrderEntity> Instance = CriteriaDefinition<OrderEntity>.Create()
        .Field(x => x.CustomerName).String().Sortable().KeywordSearchable()
        .Field(x => x.CustomerPhone, name: "phone").UsePhoneSearch(x => x.CustomerPhoneSearch, x => x.CustomerPhoneReverse)
        .Field(x => x.Status).Enum().Sortable()
        .Field(x => x.CreatedAt).DateTime().Sortable()
        .Build();
}
