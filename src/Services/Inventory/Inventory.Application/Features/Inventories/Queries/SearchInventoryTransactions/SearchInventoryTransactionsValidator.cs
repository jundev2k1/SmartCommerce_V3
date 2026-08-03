using SmartEcommerce.BuildingBlock.Criteria.Validation;

using FluentValidation;

using SmartEcommerce.Inventory.Application.Features.Inventories.Search;

namespace SmartEcommerce.Inventory.Application.Features.Inventories.Queries.SearchInventoryTransactions;

public sealed class SearchInventoryTransactionsValidator : AbstractValidator<SearchInventoryTransactionsQuery>
{
    public SearchInventoryTransactionsValidator()
    {
        RuleFor(x => x.Criteria).Custom((criteria, context) =>
        {
            var errors = CriteriaRequestValidator<InventoryTransaction>.Validate(InventoryTransactionCriteriaDefinition.Instance, criteria);
            foreach (var error in errors)
                context.AddFailure(error);
        });
    }
}
