using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;
using BuildingBlock.Contract.Events.Product;

using Product.Application.Abstractions.Persistence.Products;
using Product.Domain.ValueObjects;

namespace Product.Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductHandler(
    IProductReadService productReadService,
    IProductWriteService productWriteService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<UpdateProductCommand, UpdateProductResponse>
{
    public async Task<UpdateProductResponse> Handle(UpdateProductCommand request, CancellationToken ct = default)
    {
        if (await productReadService.SlugExistsAsync(request.Slug, request.ProductId, ct))
            throw new ConflictException($"Product with slug ({request.Slug}) already exists");

        string name = request.Name.Trim();
        string slugValue = Slug.Create(request.Slug).Value;
        var correlationId = currentUser.GetCorrelationId() ?? Guid.NewGuid().ToString();

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await productWriteService.UpdateDetailsAsync(request.ProductId, name, request.Description.Trim(), request.Slug, ct);

            await outboxStore.EnqueueAsync(
                new ProductUpdatedIntegrationEvent(request.ProductId, name, slugValue, correlationId),
                ct);
        }, ct: ct);

        return new UpdateProductResponse();
    }
}
