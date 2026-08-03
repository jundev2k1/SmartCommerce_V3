using SmartEcommerce.BuildingBlock.Application.Abstractions.Events;
using SmartEcommerce.BuildingBlock.Infrastructure.Events;

using Microsoft.Extensions.DependencyInjection;

namespace SmartEcommerce.BuildingBlock.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering event-related services.
/// Note: IEventDispatcher (for MessageQueue) is in SmartEcommerce.BuildingBlock.Messaging.Kafka - only add if service needs Kafka.
/// </summary>
public static class EventExtensions
{
    /// <summary>
    /// Registers the ApplicationEventDispatcher service for synchronous internal event handling via MediatR.
    /// This is always safe to add - only used if you publish application events.
    /// </summary>
    public static IServiceCollection AddApplicationEventDispatcher(this IServiceCollection services)
    {
        services.AddScoped<IInternalEventDispatcher, InternalEventDispatcher>();
        return services;
    }
}

