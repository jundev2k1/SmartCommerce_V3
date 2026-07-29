using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Application.Abstractions.DeadLetters;
using BuildingBlock.Criteria.Requests;
using BuildingBlock.Persistence.Ef.Criteria;

using Microsoft.EntityFrameworkCore;

using AppInboxMessageStatus = BuildingBlock.Application.Abstractions.Outbox.InboxMessageStatus;
using AppInboxRetryHistoryEntry = BuildingBlock.Application.Abstractions.Outbox.InboxRetryHistoryEntry;
using DomainInboxMessageStatus = BuildingBlock.Persistence.Inbox.InboxMessageStatus;
using DomainInboxRetryHistoryResult = BuildingBlock.Persistence.Inbox.InboxRetryHistoryResult;

namespace BuildingBlock.Persistence.Ef.Inbox;

/// <summary>
/// Generic EF implementation of IDeadLetterQueryService, parameterized over the DbContext type.
/// Mirrors EfInboxStore&lt;TContext&gt;'s "generic, registered per service DbContext" shape.
/// </summary>
public sealed class EfDeadLetterQueryService<TContext>(TContext context) : IDeadLetterQueryService
    where TContext : Microsoft.EntityFrameworkCore.DbContext, IInboxDbContext
{
    private readonly TContext _context = context;

    public async Task<PaginatedResult<DeadLetterListItemResponse>> SearchAsync(CriteriaRequest request, CancellationToken ct = default)
    {
        var query = _context.InboxMessages
            .AsNoTracking()
            .Where(x => x.Status == DomainInboxMessageStatus.DeadLetter)
            .ApplyCriteria(DeadLetterCriteriaDefinition.Instance, request);

        var paged = await query.ToCriteriaPagedResultAsync(request, ct);

        var items = paged.Items.Select(ToListItem).ToList();
        return PaginatedResult<DeadLetterListItemResponse>.Create(items, paged.PageNumber, paged.PageSize, paged.TotalCount);
    }

    public async Task<DeadLetterDetailResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var message = await _context.InboxMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (message is null)
            return null;

        var history = await _context.InboxRetryHistories
            .AsNoTracking()
            .Where(h => h.InboxMessageId == id)
            .OrderByDescending(h => h.StartedAt)
            .Select(h => new AppInboxRetryHistoryEntry(
                h.Id, h.InboxMessageId, h.MessageId, h.ConsumerName, h.Topic, h.RetryNumber,
                h.StartedAt, h.FinishedAt, h.DurationMs, h.Operator, ToApplication(h.Result), h.Exception))
            .ToListAsync(ct);

        return new DeadLetterDetailResponse(
            message.Id, message.MessageId, message.ConsumerName, message.Topic,
            message.Payload, message.HeadersJson, ToApplication(message.Status), message.RetryCount,
            message.CreatedAt, message.ProcessedAt, message.NextRetryAt, message.LastRetryAt, message.LastError,
            history);
    }

    private static DeadLetterListItemResponse ToListItem(InboxMessage m) => new(
        m.Id, m.MessageId, m.ConsumerName, m.Topic, ToApplication(m.Status), m.RetryCount, m.CreatedAt, m.LastRetryAt, m.LastError);

    private static AppInboxMessageStatus ToApplication(DomainInboxMessageStatus status) => status switch
    {
        DomainInboxMessageStatus.Pending => AppInboxMessageStatus.Pending,
        DomainInboxMessageStatus.Retrying => AppInboxMessageStatus.Retrying,
        DomainInboxMessageStatus.Processed => AppInboxMessageStatus.Processed,
        DomainInboxMessageStatus.DeadLetter => AppInboxMessageStatus.DeadLetter,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
    };

    private static BuildingBlock.Application.Abstractions.Outbox.InboxRetryHistoryResult ToApplication(DomainInboxRetryHistoryResult result) => result switch
    {
        DomainInboxRetryHistoryResult.Retrying => BuildingBlock.Application.Abstractions.Outbox.InboxRetryHistoryResult.Retrying,
        DomainInboxRetryHistoryResult.Succeeded => BuildingBlock.Application.Abstractions.Outbox.InboxRetryHistoryResult.Succeeded,
        DomainInboxRetryHistoryResult.FailedAgain => BuildingBlock.Application.Abstractions.Outbox.InboxRetryHistoryResult.FailedAgain,
        DomainInboxRetryHistoryResult.Cancelled => BuildingBlock.Application.Abstractions.Outbox.InboxRetryHistoryResult.Cancelled,
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null),
    };
}
