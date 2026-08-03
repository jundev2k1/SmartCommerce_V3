using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Application.Abstractions.CQRS;
using BuildingBlock.Application.Abstractions.DeadLetters;
using BuildingBlock.Criteria.Requests;

namespace BuildingBlock.Infrastructure.DeadLetters.Queries;

public sealed record SearchDeadLettersQuery(CriteriaRequest Criteria) : IQuery<PaginatedResult<DeadLetterListItemResponse>>;

public sealed class SearchDeadLettersHandler(IDeadLetterQueryService queryService)
    : IQueryHandler<SearchDeadLettersQuery, PaginatedResult<DeadLetterListItemResponse>>
{
    public Task<PaginatedResult<DeadLetterListItemResponse>> Handle(SearchDeadLettersQuery request, CancellationToken ct = default) =>
        queryService.SearchAsync(request.Criteria, ct);
}
