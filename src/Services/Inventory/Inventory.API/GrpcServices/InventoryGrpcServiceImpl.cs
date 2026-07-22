using BuildingBlock.Contract.Protos.Inventory;

using Grpc.Core;

using Inventory.Application.Features.Inventories.Commands.RestockStock;

using AppDeductStockItem = Inventory.Application.Features.Inventories.Commands.DeductStock.DeductStockItem;
using DeductStockCommand = Inventory.Application.Features.Inventories.Commands.DeductStock.DeductStockCommand;
using GetProductStockQuery = Inventory.Application.Features.Inventories.Queries.GetProductStock.GetProductStockQuery;
using GetProductsStockQuery = Inventory.Application.Features.Inventories.Queries.GetProductsStock.GetProductsStockQuery;

namespace Inventory.API.GrpcServices;

/// <summary>Thin adapter for Order Service (or any other gRPC caller) - parses the request, dispatches the same query/commands the REST endpoints use, no business logic here.</summary>
public sealed class InventoryGrpcServiceImpl(ISender sender) : InventoryGrpcService.InventoryGrpcServiceBase
{
    public override async Task<GetProductStockResponse> GetProductStock(GetProductStockRequest request, ServerCallContext context)
    {
        Guid? productVariationId = request.HasProductVariationId && !string.IsNullOrEmpty(request.ProductVariationId)
            ? Guid.Parse(request.ProductVariationId)
            : null;

        var query = new GetProductStockQuery(Guid.Parse(request.ProductId), productVariationId);
        var result = await sender.Send(query, context.CancellationToken);

        return new GetProductStockResponse
        {
            ProductId = result.ProductId.ToString(),
            ProductVariationId = result.ProductVariationId?.ToString() ?? string.Empty,
            TotalQuantity = result.TotalQuantity,
        };
    }

    public override async Task<GetProductsStockResponse> GetProductsStock(GetProductsStockRequest request, ServerCallContext context)
    {
        var variationIds = request.ProductVariationIds.Select(Guid.Parse).ToArray();

        var query = new GetProductsStockQuery(variationIds);
        var result = await sender.Send(query, context.CancellationToken);

        var response = new GetProductsStockResponse();
        response.Items.AddRange(result.Select(r => new ProductVariationStock
        {
            ProductVariationId = r.ProductVariationId.ToString(),
            TotalQuantity = r.TotalQuantity,
        }));

        return response;
    }

    public override async Task<DeductStockResponse> DeductStock(DeductStockRequest request, ServerCallContext context)
    {
        var items = request.Items
            .Select(i => new AppDeductStockItem(Guid.Parse(i.ProductVariationId), i.Quantity))
            .ToList();

        var command = new DeductStockCommand(
            Guid.Parse(request.DeductionId),
            items,
            string.IsNullOrEmpty(request.Reason) ? null : request.Reason);

        var result = await sender.Send(command, context.CancellationToken);

        var response = new DeductStockResponse
        {
            Success = result.Success,
            FailureCode = result.FailureCode ?? string.Empty,
        };

        response.InsufficientItems.AddRange(result.InsufficientItems.Select(i => new BuildingBlock.Contract.Protos.Inventory.InsufficientStockItem
        {
            ProductVariationId = i.ProductVariationId.ToString(),
            RequestedQuantity = i.RequestedQuantity,
            AvailableQuantity = i.AvailableQuantity,
        }));

        return response;
    }

    public override async Task<RestockStockResponse> RestockStock(RestockStockRequest request, ServerCallContext context)
    {
        var command = new RestockStockCommand(
            Guid.Parse(request.DeductionId),
            string.IsNullOrEmpty(request.Reason) ? null : request.Reason);

        var result = await sender.Send(command, context.CancellationToken);

        return new RestockStockResponse { Success = result.Success };
    }
}
