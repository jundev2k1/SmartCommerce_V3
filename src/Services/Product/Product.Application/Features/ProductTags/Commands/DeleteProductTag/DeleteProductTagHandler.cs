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
        _ = await tagReadService.GetByIdAsync(request.ProductTagId, ct)
            ?? throw new NotFoundException(nameof(ProductTag), request.ProductTagId);

        if (await productReadService.ExistsWithTagAsync(request.ProductTagId, ct))
            throw new ConflictException("Cannot delete a tag that is still assigned to products.");

        await tagWriteService.DeleteAsync(request.ProductTagId, ct);

        return new DeleteProductTagResponse();
    }
}
