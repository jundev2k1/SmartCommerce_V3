using FluentValidation;

using Order.Application.Features.Orders.Commands.CreateOrder;

namespace Order.Application.Features.Orders.Commands.UpdateOrder;

public sealed class UpdateOrderValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId is required");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item")
            .Must(items => items.Count <= CreateOrderValidator.MaxItemsPerOrder)
                .WithMessage($"Order cannot contain more than {CreateOrderValidator.MaxItemsPerOrder} items")
            .Must(items => items.Select(i => i.VariationId).Distinct().Count() == items.Count)
                .WithMessage("Order items must not contain duplicate products/variations");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("ProductId is required");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0")
                .LessThanOrEqualTo(CreateOrderValidator.MaxQuantityPerItem).WithMessage($"Quantity cannot exceed {CreateOrderValidator.MaxQuantityPerItem} per item");

            item.RuleFor(i => i.Discount)
                .GreaterThanOrEqualTo(0).WithMessage("Discount cannot be negative");
        });
    }
}
