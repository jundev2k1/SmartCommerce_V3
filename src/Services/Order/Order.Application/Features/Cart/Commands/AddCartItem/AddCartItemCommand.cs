using Order.Application.Abstractions.Services;

namespace Order.Application.Features.Cart.Commands.AddCartItem;

public sealed record AddCartItemCommand(Guid VariationId, int Quantity) : ICommand<CartResponse>;
