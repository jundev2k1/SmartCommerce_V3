using FluentValidation;

namespace Order.Application.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public const int MaxItemsPerOrder = 50;
    public const int MaxQuantityPerItem = 100;

    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("CustomerName is required")
            .MaximumLength(200);

        RuleFor(x => x.CustomerPhone)
            .NotEmpty().WithMessage("CustomerPhone is required")
            .MaximumLength(30);

        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("ShippingAddress is required")
            .MaximumLength(500);

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item")
            .Must(items => items.Count <= MaxItemsPerOrder)
                .WithMessage($"Order cannot contain more than {MaxItemsPerOrder} items")
            .Must(items => items.Select(i => i.VariationId).Distinct().Count() == items.Count)
                .WithMessage("Order items must not contain duplicate products/variations");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("ProductId is required");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0")
                .LessThanOrEqualTo(MaxQuantityPerItem).WithMessage($"Quantity cannot exceed {MaxQuantityPerItem} per item");

            item.RuleFor(i => i.Discount)
                .GreaterThanOrEqualTo(0).WithMessage("Discount cannot be negative");
        });
    }
}
