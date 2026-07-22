using Order.Application.Abstractions.Services;

namespace Order.Application.Features.Cart.Commands.RemoveCartItem;

public sealed record RemoveCartItemCommand(Guid VariationId) : ICommand<CartResponse>;
