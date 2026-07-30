using Mapster;

using Order.Application.Abstractions.Services;
using Order.Application.Features.Orders.DTOs;

namespace Order.Application.Features.Orders.Mapping;

public sealed class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig cfg)
    {
        cfg.NewConfig<CartItemResponse, OrderItemRequestDto>()
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.VariationId, src => src.VariationId)
            .Map(dest => dest.Quantity, src => src.Quantity);
    }
}
