using MediatR;

namespace BuildingBlock.Application.Abstractions.CQRS;

public interface ICommand : IRequest
{
}

public interface ICommand<TResponse> : IRequest<TResponse>
{
}
