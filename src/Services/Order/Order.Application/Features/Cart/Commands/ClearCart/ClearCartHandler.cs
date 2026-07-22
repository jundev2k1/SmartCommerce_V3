using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;

using Order.Application.Abstractions.Services;

namespace Order.Application.Features.Cart.Commands.ClearCart;

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
