using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
using SmartEcommerce.BuildingBlock.Application.Exceptions;

using SmartEcommerce.Order.Application.Abstractions.Services;

namespace SmartEcommerce.Order.Application.Features.Cart.Queries.GetCart;

public sealed class GetCartHandler(
    ICurrentUserService currentUser,
    ICartService cartService) : IQueryHandler<GetCartQuery, CartResponse>
{
    public async Task<CartResponse> Handle(GetCartQuery request, CancellationToken ct = default)
    {
        var userId = currentUser.GetUserId()
            ?? throw new ForbiddenException();

        var (_, cart) = await cartService.GetCartAsync(userId, ct);
        return cart;
    }
}
