using SmartEcommerce.Order.Application.Abstractions.Services;

namespace SmartEcommerce.Order.Application.Features.Cart.Queries.GetCart;

public sealed record GetCartQuery : IQuery<CartResponse>;
