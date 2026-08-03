using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
using SmartEcommerce.BuildingBlock.Application.Exceptions;

using SmartEcommerce.Order.Application.Abstractions.Services;

namespace SmartEcommerce.Order.Application.Features.Cart.Commands.UpdateCartItemQuantity;

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
