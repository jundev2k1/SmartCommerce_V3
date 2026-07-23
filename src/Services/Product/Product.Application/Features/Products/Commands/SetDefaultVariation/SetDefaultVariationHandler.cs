using Product.Application.Abstractions.Persistence.Products;

namespace Product.Application.Features.Products.Commands.SetDefaultVariation;

public sealed class SetDefaultVariationHandler(
    IProductWriteService productWriteService) : ICommandHandler<SetDefaultVariationCommand, SetDefaultVariationResponse>
{
    public async Task<SetDefaultVariationResponse> Handle(SetDefaultVariationCommand request, CancellationToken ct = default)
    {
        await productWriteService.UpdateAsync(request.ProductId, async (product) =>
        {
            product.SetDefaultVariation(request.VariationId);
            await Task.CompletedTask;
        }, ct);

        return new SetDefaultVariationResponse();
    }
}
