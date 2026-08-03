using SmartEcommerce.BuildingBlock.Application.Abstractions.Events;

using SmartEcommerce.Product.Application.Abstractions.Search;

namespace SmartEcommerce.Product.Application.Features.Products.Events.OnProductSearchRemovalRequired;

public sealed class OnProductSearchRemovalRequiredHandler(IProductSearchIndexer searchIndexer)
    : IInternalEventHandler<OnProductSearchRemovalRequiredEvent>
{
    public Task Handle(OnProductSearchRemovalRequiredEvent @event, CancellationToken ct = default)
        => searchIndexer.DeleteAsync(@event.ProductId, ct);
}
