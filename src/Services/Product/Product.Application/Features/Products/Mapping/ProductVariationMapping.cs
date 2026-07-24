using Product.Application.Features.Products.DTOs;
using Product.Domain.ValueObjects;

namespace Product.Application.Features.Products.Mapping;

public static class ProductVariationMapping
{
    public static ProductVariation MapInputToEntity(
        this ProductVariationInputDto dto,
        Guid productId,
        int displayOrder)
    {
        return ProductVariation.Create(
            productId: productId,
            sku: Sku.Create(dto.Sku),
            price: dto.Price,
            displayOrder: displayOrder,
            barcode: dto.Barcode is not null ? Barcode.Create(dto.Barcode) : null,
            cost: dto.Cost,
            weight: dto.Weight,
            dimensions: dto.DimensionsLength.HasValue
                && dto.DimensionsWidth.HasValue
                && dto.DimensionsHeight.HasValue
                    ? Dimensions.Create(
                        dto.DimensionsLength.Value,
                        dto.DimensionsWidth.Value,
                        dto.DimensionsHeight.Value)
                    : null,
            images: dto.Images,
            status: ProductVariationStatus.Active,
            metadata: null);
    }

    public static IEnumerable<ProductVariation> MapInputToEntities(
        this IEnumerable<ProductVariationInputDto> dtos,
        Guid productId,
        int beginDisplayOrder = 1)
    {
        foreach (var dto in dtos)
        {
            var entity = dto.MapInputToEntity(productId, beginDisplayOrder);
            if (dto.IsDefault)
                entity.MarkAsDefault();

            beginDisplayOrder++;
            yield return entity;
        }
    }
}
