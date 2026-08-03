using SmartEcommerce.BuildingBlock.Criteria.Validation;
using FluentValidation;

using SmartEcommerce.Order.Application.Features.Orders.Search;

namespace SmartEcommerce.Order.Application.Features.Orders.Queries.SearchOrders;

public sealed class SearchOrdersValidator : AbstractValidator<SearchOrdersQuery>
{
    public SearchOrdersValidator()
    {
        RuleFor(x => x.Criteria).Custom((criteria, context) =>
        {
            var errors = CriteriaRequestValidator<OrderEntity>.Validate(OrderCriteriaDefinition.Instance, criteria);
            foreach (var error in errors)
                context.AddFailure(error);
        });
    }
}
