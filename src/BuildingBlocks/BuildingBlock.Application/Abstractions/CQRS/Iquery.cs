using MediatR;

namespace BuildingBlock.Application.Abstractions.CQRS;

public interface IQuery<TResponse> : IRequest<TResponse>
{
}