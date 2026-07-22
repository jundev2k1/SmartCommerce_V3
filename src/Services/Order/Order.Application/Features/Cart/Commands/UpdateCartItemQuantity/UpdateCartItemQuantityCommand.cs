using Order.Application.Abstractions.Services;

namespace Order.Application.Features.Cart.Commands.UpdateCartItemQuantity;

public sealed record UpdateCartItemQuantityCommand(Guid VariationId, int Quantity) : ICommand<CartResponse>;
