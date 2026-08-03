using BuildingBlock.Application.Abstractions.Services;

using Inventory.Application.Abstractions.Services;

namespace Inventory.Application.Features.Inventories.Commands.CycleCount;

public sealed class CompleteCycleCountHandler(
    ICycleCountService cycleCountService,
    IUnitOfWork unitOfWork,
    IAppLogger<CompleteCycleCountHandler> logger) : ICommandHandler<CompleteCycleCountCommand, CompleteCycleCountResponse>
{
    public async Task<CompleteCycleCountResponse> Handle(CompleteCycleCountCommand request, CancellationToken ct = default)
    {
        ICycleCountService.CycleCountResult? result = null;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var items = request.CountedItems
                .Select(i => new ICycleCountService.CountItem(i.ProductVariantId, i.ActualQuantity))
                .ToList();

            result = await cycleCountService.CompleteCountAsync(
                request.CountId,
                items,
                request.VarianceThresholdPercent,
                ct);

            logger.Information(
                "Completed cycle count {CountNumber}: {ItemCount} items, {VarianceCount} with variance, {AdjustmentCount} adjustments",
                result.CountDocument.Number,
                request.CountedItems.Count,
                result.ItemsWithVariance,
                result.ItemsAdjusted);
        }, ct: ct);

        var totalVariance = result!.Variances.Sum(v => Math.Abs(v.Variance));

        return new CompleteCycleCountResponse(
            TotalItemsCounted: request.CountedItems.Count,
            ItemsWithVariance: result.ItemsWithVariance,
            ItemsAdjusted: result.ItemsAdjusted,
            TotalVarianceValue: totalVariance,
            Variances: result.Variances
                .Select(v => new VarianceItem(
                    ProductVariantId: v.ProductVariantId,
                    ExpectedQuantity: v.ExpectedQuantity,
                    ActualQuantity: v.ActualQuantity,
                    Variance: v.Variance,
                    VariancePercent: v.VariancePercent))
                .ToList());
    }
}
