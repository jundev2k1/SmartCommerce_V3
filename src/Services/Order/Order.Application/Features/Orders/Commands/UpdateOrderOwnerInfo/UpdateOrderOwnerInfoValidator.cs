using FluentValidation;

using Order.Domain.ValueObjects;

namespace Order.Application.Features.Orders.Commands.UpdateOrderOwnerInfo;

public sealed class UpdateOrderOwnerInfoValidator : AbstractValidator<UpdateOrderOwnerInfoCommand>
{
    public UpdateOrderOwnerInfoValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId is required");

        RuleFor(x => x.OwnerPhone)
            .Must(PhoneNumber.IsValid).WithMessage("OwnerPhone is not valid");

        RuleFor(x => x.OwnerEmail)
            .Must(Email.IsValid).WithMessage("OwnerEmail is not valid");
    }
}
