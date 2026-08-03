using MediatR;

namespace SmartEcommerce.BuildingBlock.Application.Abstractions.CQRS;

public interface IQuery<TResponse> : IRequest<TResponse>
{
}