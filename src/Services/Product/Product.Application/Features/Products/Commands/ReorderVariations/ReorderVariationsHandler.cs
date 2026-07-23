using Product.Application.Abstractions.Persistence.Products;

namespace Product.Application.Features.Products.Commands.ReorderVariations;

public sealed class ReorderVariationsHandler(
    IProductWriteService productWriteService,
    IUnitOfWork unitOfWork) : ICommandHandler<ReorderVariationsCommand, ReorderVariationsResponse>
{
    public async Task<ReorderVariationsResponse> Handle(ReorderVariationsCommand request, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await productWriteService.ReorderVariationsAsync(request.ProductId, request.OrderedVariationIds, ct);
        }, ct: ct);

        return new ReorderVariationsResponse();
    }
}
