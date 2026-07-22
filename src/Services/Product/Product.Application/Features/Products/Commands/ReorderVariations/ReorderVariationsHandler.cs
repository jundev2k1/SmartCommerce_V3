using BuildingBlock.Application.Exceptions;

using Product.Application.Abstractions.Repositories;

namespace Product.Application.Features.Products.Commands.ReorderVariations;

public sealed class ReorderVariationsHandler(
    IProductRepository productRepo,
    IUnitOfWork unitOfWork) : ICommandHandler<ReorderVariationsCommand, ReorderVariationsResponse>
{
    public async Task<ReorderVariationsResponse> Handle(ReorderVariationsCommand request, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await productRepo.UpdateAsync(request.ProductId, async (product) =>
            {
                var currentIds = product.Variations.Select(v => v.Id).ToHashSet();
                var requestedIds = request.OrderedVariationIds.ToHashSet();

                if (requestedIds.Count != request.OrderedVariationIds.Count || !currentIds.SetEquals(requestedIds))
                {
                    throw new BadRequestException(
                        "OrderedVariationIds must contain exactly every existing variation id, each exactly once.");
                }

                for (var i = 0; i < request.OrderedVariationIds.Count; i++)
                {
                    var variation = product.Variations.First(v => v.Id == request.OrderedVariationIds[i]);
                    variation.ChangeDisplayOrder(i);
                }

                await Task.CompletedTask;
            }, ct);
        }, ct: ct);

        return new ReorderVariationsResponse();
    }
}
