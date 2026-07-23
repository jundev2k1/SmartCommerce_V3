using Product.Application.Abstractions.Persistence.Products;

namespace Product.Application.Features.Products.Commands.SetDefaultVariation;

public sealed class SetDefaultVariationHandler(
    IProductWriteService productWriteService,
    IUnitOfWork unitOfWork) : ICommandHandler<SetDefaultVariationCommand, SetDefaultVariationResponse>
{
    public async Task<SetDefaultVariationResponse> Handle(SetDefaultVariationCommand request, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await productWriteService.SetDefaultVariationAsync(request.ProductId, request.VariationId, ct);
        }, ct: ct);

        return new SetDefaultVariationResponse();
    }
}
