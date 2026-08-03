using SmartEcommerce.BuildingBlock.Application.Abstractions.CQRS;
using SmartEcommerce.BuildingBlock.Application.Abstractions.DeadLetters;
using SmartEcommerce.BuildingBlock.Application.Exceptions;

namespace SmartEcommerce.BuildingBlock.Infrastructure.DeadLetters.Queries;

public sealed record GetDeadLetterQuery(Guid Id) : IQuery<DeadLetterDetailResponse>;

public sealed class GetDeadLetterHandler(IDeadLetterQueryService queryService)
    : IQueryHandler<GetDeadLetterQuery, DeadLetterDetailResponse>
{
    public async Task<DeadLetterDetailResponse> Handle(GetDeadLetterQuery request, CancellationToken ct = default) =>
        await queryService.GetByIdAsync(request.Id, ct)
        ?? throw new NotFoundException("DeadLetterMessage", request.Id);
}
