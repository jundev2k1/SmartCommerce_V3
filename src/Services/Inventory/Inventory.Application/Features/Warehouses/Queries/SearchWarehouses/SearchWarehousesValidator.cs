using SmartEcommerce.BuildingBlock.Criteria.Validation;

using FluentValidation;

using SmartEcommerce.Inventory.Application.Features.Warehouses.Search;

namespace SmartEcommerce.Inventory.Application.Features.Warehouses.Queries.SearchWarehouses;

public sealed class SearchWarehousesValidator : AbstractValidator<SearchWarehousesQuery>
{
    public SearchWarehousesValidator()
    {
        RuleFor(x => x.Criteria).Custom((criteria, context) =>
        {
            var errors = CriteriaRequestValidator<Warehouse>.Validate(WarehouseCriteriaDefinition.Instance, criteria);
            foreach (var error in errors)
                context.AddFailure(error);
        });
    }
}
