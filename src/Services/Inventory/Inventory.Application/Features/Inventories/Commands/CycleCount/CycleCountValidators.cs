using FluentValidation;

namespace Inventory.Application.Features.Inventories.Commands.CycleCount;

public sealed class StartCycleCountValidator : AbstractValidator<StartCycleCountCommand>
{
    public StartCycleCountValidator()
    {
        RuleFor(x => x.WarehouseId)
            .NotEmpty()
            .WithMessage("Warehouse ID is required.");

        RuleFor(x => x.CountDate)
            .NotEmpty()
            .WithMessage("Count date is required.");

        RuleFor(x => x.CountDate)
            .Must(BeValidDate)
            .WithMessage("Count date must be a valid date.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters.");
    }

    private static bool BeValidDate(string date)
    {
        return DateTime.TryParse(date, out _);
    }
}

public sealed class CompleteCycleCountValidator : AbstractValidator<CompleteCycleCountCommand>
{
    public CompleteCycleCountValidator()
    {
        RuleFor(x => x.CountId)
            .NotEmpty()
            .WithMessage("Cycle count ID is required.");

        RuleFor(x => x.CountedItems)
            .NotEmpty()
            .WithMessage("At least one item must be counted.");

        RuleForEach(x => x.CountedItems)
            .SetValidator(new CountItemValidator());

        RuleFor(x => x.VarianceThresholdPercent)
            .GreaterThan(0)
            .WithMessage("Variance threshold must be greater than 0.");

        RuleFor(x => x.VarianceThresholdPercent)
            .LessThanOrEqualTo(100)
            .WithMessage("Variance threshold must not exceed 100 percent.");
    }
}

public sealed class CountItemValidator : AbstractValidator<CycleCountItemRequest>
{
    public CountItemValidator()
    {
        RuleFor(x => x.ProductVariantId)
            .NotEmpty()
            .WithMessage("Product variant ID is required.");

        RuleFor(x => x.ActualQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Actual quantity must be 0 or greater.");
    }
}
