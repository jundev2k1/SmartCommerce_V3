using BuildingBlock.Application.Exceptions;

using Product.Application.Abstractions.Persistence.Products;
using Product.Application.Abstractions.Persistence.ProductTags;

namespace Product.Application.Features.ProductTags.Commands.DeleteProductTag;

public sealed class DeleteProductTagHandler(
    IProductTagReadService tagReadService,
    IProductTagWriteService tagWriteService,
    IProductReadService productReadService) : ICommandHandler<DeleteProductTagCommand, DeleteProductTagResponse>
{
    public async Task<DeleteProductTagResponse> Handle(DeleteProductTagCommand request, CancellationToken ct = default)
    {
        var isExist = await tagReadService.IsExistAsync(request.ProductTagId, ct);
        if (!isExist)
            throw new NotFoundException(nameof(ProductTag), request.ProductTagId);

        // TODO: Add detail prop
        var existingProductIds = await productReadService.GetProductsByTagIdAsync(request.ProductTagId, ct);
        if (existingProductIds.Length > 0)
            throw new ConflictException(
                systemMessage: "Cannot delete a tag that is still assigned to products.",
                detail: new { existingProductIds });

        await tagWriteService.DeleteAsync(request.ProductTagId, ct);

        return new DeleteProductTagResponse();
    }
}
