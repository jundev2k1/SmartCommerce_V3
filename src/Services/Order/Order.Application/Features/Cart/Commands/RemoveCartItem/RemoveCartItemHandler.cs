using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;

using Order.Application.Abstractions.Services;

namespace Order.Application.Features.Cart.Commands.RemoveCartItem;

public sealed class RemoveCartItemHandler(
    ICurrentUserService currentUser,
    ICartService cartService) : ICommandHandler<RemoveCartItemCommand, CartResponse>
{
    public async Task<CartResponse> Handle(RemoveCartItemCommand request, CancellationToken ct = default)
    {
        var userId = currentUser.GetUserId() ?? throw new ForbiddenException();

        return await cartService.RemoveItemAsync(userId, request.VariationId, ct);
    }
}
