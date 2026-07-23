using Product.Application.Abstractions.Persistence.ProductTags;

namespace Product.Application.Features.ProductTags.Commands.UpdateProductTag;

public sealed class UpdateProductTagHandler(
    IProductTagWriteService tagWriteService) : ICommandHandler<UpdateProductTagCommand, UpdateProductTagResponse>
{
    public async Task<UpdateProductTagResponse> Handle(UpdateProductTagCommand request, CancellationToken ct = default)
    {
        await tagWriteService.UpdateAsync(request.ProductTagId, async (tag) =>
        {
            tag.Rename(request.Name.Trim());
            await Task.CompletedTask;
        }, ct);

        return new UpdateProductTagResponse();
    }
}
