using MediatR;

namespace SmartEcommerce.BuildingBlock.Application.Abstractions.CQRS;

public interface ICommand : IRequest
{
}

public interface ICommand<TResponse> : IRequest<TResponse>
{
}
