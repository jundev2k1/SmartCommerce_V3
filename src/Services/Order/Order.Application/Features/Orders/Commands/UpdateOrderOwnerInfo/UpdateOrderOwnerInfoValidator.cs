using FluentValidation;

namespace Order.Application.Features.Orders.Commands.UpdateOrderOwnerInfo;

public sealed class UpdateOrderOwnerInfoValidator : AbstractValidator<UpdateOrderOwnerInfoCommand>
{
    public UpdateOrderOwnerInfoValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId is required");

        RuleFor(x => x.CustomerPhone)
            .NotEmpty().WithMessage("CustomerPhone is required")
            .MaximumLength(30);

        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("ShippingAddress is required")
            .MaximumLength(500);
    }
}
