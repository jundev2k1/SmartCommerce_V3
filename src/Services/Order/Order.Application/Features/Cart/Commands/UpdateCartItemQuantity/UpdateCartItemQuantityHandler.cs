using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;

using Order.Application.Abstractions.Services;

namespace Order.Application.Features.Cart.Commands.UpdateCartItemQuantity;

public sealed class UpdateCartItemQuantityHandler(
    ICurrentUserService currentUser,
    ICartService cartService) : ICommandHandler<UpdateCartItemQuantityCommand, CartResponse>
{
    public async Task<CartResponse> Handle(UpdateCartItemQuantityCommand request, CancellationToken ct = default)
    {
        var userId = currentUser.GetUserId() ?? throw new ForbiddenException();

        return await cartService.UpdateItemQuantityAsync(userId, request.VariationId, request.Quantity, ct);
    }
}
