using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
using SmartEcommerce.BuildingBlock.Application.Exceptions;

using SmartEcommerce.Order.Application.Abstractions.Services;

namespace SmartEcommerce.Order.Application.Features.Cart.Commands.ClearCart;

public sealed class ClearCartHandler(
    ICurrentUserService currentUser,
    ICartService cartService) : ICommandHandler<ClearCartCommand>
{
    public async Task Handle(ClearCartCommand request, CancellationToken ct = default)
    {
        var userId = currentUser.GetUserId() ?? throw new ForbiddenException();

        await cartService.ClearCartAsync(userId, ct);
    }
}
