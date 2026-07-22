using Order.Application.Abstractions.Services;

namespace Order.Application.Features.Cart.Queries.GetCart;

public sealed record GetCartQuery : IQuery<CartResponse>;
