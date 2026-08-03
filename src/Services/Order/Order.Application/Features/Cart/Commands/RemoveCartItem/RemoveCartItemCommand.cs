using SmartEcommerce.Order.Application.Abstractions.Services;

namespace SmartEcommerce.Order.Application.Features.Cart.Commands.RemoveCartItem;

public sealed record RemoveCartItemCommand(Guid VariationId) : ICommand<CartResponse>;
