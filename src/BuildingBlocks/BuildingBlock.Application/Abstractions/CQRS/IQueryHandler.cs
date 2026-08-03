using MediatR;

namespace SmartEcommerce.BuildingBlock.Application.Abstractions.CQRS;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}