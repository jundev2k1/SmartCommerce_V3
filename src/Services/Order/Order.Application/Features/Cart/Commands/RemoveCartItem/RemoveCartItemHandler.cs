using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
using SmartEcommerce.BuildingBlock.Application.Exceptions;

using SmartEcommerce.Order.Application.Abstractions.Services;

namespace SmartEcommerce.Order.Application.Features.Cart.Commands.RemoveCartItem;

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
