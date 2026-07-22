using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;
using BuildingBlock.Contract.Events.Product;

using Product.Application.Abstractions.Repositories;

namespace Product.Application.Features.Products.Commands.AssignProductTag;

public sealed class AssignProductTagHandler(
    IProductRepository productRepo,
    IProductTagRepository tagRepo,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<AssignProductTagCommand, AssignProductTagResponse>
{
    public async Task<AssignProductTagResponse> Handle(AssignProductTagCommand request, CancellationToken ct = default)
    {
        _ = await tagRepo.GetByIdAsync(request.TagId, ct)
            ?? throw new NotFoundException(nameof(ProductTag), request.TagId);

        var correlationId = currentUser.GetCorrelationId() ?? Guid.NewGuid().ToString();

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await productRepo.UpdateAsync(request.ProductId, async (product) =>
            {
                product.AssignTag(request.TagId);
                await Task.CompletedTask;
            }, ct);

            await outboxStore.EnqueueAsync(
                new ProductTagAssignedIntegrationEvent(request.ProductId, request.TagId, correlationId), ct);
        }, ct: ct);

        return new AssignProductTagResponse();
    }
}
