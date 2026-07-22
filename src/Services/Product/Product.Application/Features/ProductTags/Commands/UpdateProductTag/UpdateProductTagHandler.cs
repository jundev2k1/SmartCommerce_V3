using Product.Application.Abstractions.Repositories;

namespace Product.Application.Features.ProductTags.Commands.UpdateProductTag;

public sealed class UpdateProductTagHandler(
    IProductTagRepository tagRepo,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateProductTagCommand, UpdateProductTagResponse>
{
    public async Task<UpdateProductTagResponse> Handle(UpdateProductTagCommand request, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await tagRepo.UpdateAsync(request.ProductTagId, async (tag) =>
            {
                tag.Rename(request.Name.Trim());
                await Task.CompletedTask;
            }, ct);
        }, ct: ct);

        return new UpdateProductTagResponse();
    }
}
