using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Application.Abstractions.CQRS;
using SmartEcommerce.BuildingBlock.Application.Abstractions.DeadLetters;
using SmartEcommerce.BuildingBlock.Criteria.Requests;

namespace SmartEcommerce.BuildingBlock.Infrastructure.DeadLetters.Queries;

public sealed record SearchDeadLettersQuery(CriteriaRequest Criteria) : IQuery<PaginatedResult<DeadLetterListItemResponse>>;

public sealed class SearchDeadLettersHandler(IDeadLetterQueryService queryService)
    : IQueryHandler<SearchDeadLettersQuery, PaginatedResult<DeadLetterListItemResponse>>
{
    public Task<PaginatedResult<DeadLetterListItemResponse>> Handle(SearchDeadLettersQuery request, CancellationToken ct = default) =>
        queryService.SearchAsync(request.Criteria, ct);
}
