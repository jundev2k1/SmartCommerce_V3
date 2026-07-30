using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;

using Order.Application.Abstractions.Services;

namespace Order.Application.Features.Cart.Queries.GetCart;

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
