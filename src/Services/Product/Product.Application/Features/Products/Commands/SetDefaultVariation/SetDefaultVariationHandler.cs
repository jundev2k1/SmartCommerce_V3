using Product.Application.Abstractions.Repositories;

namespace Product.Application.Features.Products.Commands.SetDefaultVariation;

public sealed class SetDefaultVariationHandler(
    IProductRepository productRepo,
    IUnitOfWork unitOfWork) : ICommandHandler<SetDefaultVariationCommand, SetDefaultVariationResponse>
{
    public async Task<SetDefaultVariationResponse> Handle(SetDefaultVariationCommand request, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await productRepo.UpdateAsync(request.ProductId, async (product) =>
            {
                product.SetDefaultVariation(request.VariationId);
                await Task.CompletedTask;
            }, ct);
        }, ct: ct);

        return new SetDefaultVariationResponse();
    }
}
