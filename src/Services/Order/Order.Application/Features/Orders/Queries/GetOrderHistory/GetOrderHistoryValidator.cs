using SmartEcommerce.BuildingBlock.Criteria.Validation;
using FluentValidation;

using SmartEcommerce.Order.Application.Features.Orders.Search;

namespace SmartEcommerce.Order.Application.Features.Orders.Queries.GetOrderHistory;

public sealed class GetOrderHistoryValidator : AbstractValidator<GetOrderHistoryQuery>
{
    public GetOrderHistoryValidator()
    {
        RuleFor(x => x.Criteria).Custom((criteria, context) =>
        {
            var errors = CriteriaRequestValidator<OrderEntity>.Validate(OrderHistoryCriteriaDefinition.Instance, criteria);
            foreach (var error in errors)
                context.AddFailure(error);
        });
    }
}
