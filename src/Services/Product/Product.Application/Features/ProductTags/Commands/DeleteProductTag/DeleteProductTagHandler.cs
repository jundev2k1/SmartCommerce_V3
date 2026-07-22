using BuildingBlock.Application.Exceptions;

using Product.Application.Abstractions.Repositories;

namespace Product.Application.Features.ProductTags.Commands.DeleteProductTag;

public sealed class DeleteProductTagHandler(
    IProductTagRepository tagRepo,
    IProductRepository productRepo,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteProductTagCommand, DeleteProductTagResponse>
{
    public async Task<DeleteProductTagResponse> Handle(DeleteProductTagCommand request, CancellationToken ct = default)
    {
        _ = await tagRepo.GetByIdAsync(request.ProductTagId, ct)
            ?? throw new NotFoundException(nameof(ProductTag), request.ProductTagId);

        if (await productRepo.ExistsWithTagAsync(request.ProductTagId, ct))
            throw new ConflictException("Cannot delete a tag that is still assigned to products.");

        await tagRepo.DeleteAsync(request.ProductTagId, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new DeleteProductTagResponse();
    }
}
