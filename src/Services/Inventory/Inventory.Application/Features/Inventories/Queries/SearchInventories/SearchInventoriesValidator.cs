using SmartEcommerce.BuildingBlock.Criteria.Validation;

using FluentValidation;

using SmartEcommerce.Inventory.Application.Features.Inventories.Search;

namespace SmartEcommerce.Inventory.Application.Features.Inventories.Queries.SearchInventories;

public sealed class SearchInventoriesValidator : AbstractValidator<SearchInventoriesQuery>
{
    public SearchInventoriesValidator()
    {
        RuleFor(x => x.Criteria).Custom((criteria, context) =>
        {
            var errors = CriteriaRequestValidator<InventoryStock>.Validate(InventoryCriteriaDefinition.Instance, criteria);
            foreach (var error in errors)
                context.AddFailure(error);
        });
    }
}
