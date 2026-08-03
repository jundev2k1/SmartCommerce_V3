using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
using SmartEcommerce.BuildingBlock.Application.Exceptions;

using SmartEcommerce.Order.Application.Abstractions.Services;

namespace SmartEcommerce.Order.Application.Features.Cart.Commands.AddCartItem;

public sealed class AddCartItemHandler(
    ICurrentUserService currentUser,
    ICartService cartService) : ICommandHandler<AddCartItemCommand, CartResponse>
{
    public async Task<CartResponse> Handle(AddCartItemCommand request, CancellationToken ct = default)
    {
        var userId = currentUser.GetUserId() ?? throw new ForbiddenException();

        return await cartService.AddItemAsync(userId, request.VariationId, request.Quantity, ct);
    }
}
