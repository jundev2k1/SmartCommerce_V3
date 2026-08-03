using SmartEcommerce.Notification.Application.Abstractions.Services;
using SmartEcommerce.Notification.Domain.Enums;

namespace SmartEcommerce.Notification.Infrastructure.Delivery;

public sealed class ChannelSenderResolver(IEnumerable<IChannelSender> senders) : IChannelSenderResolver
{
    private readonly Dictionary<NotificationChannelType, IChannelSender> _senders =
        senders.ToDictionary(s => s.ChannelType);

    public IChannelSender Resolve(NotificationChannelType channelType) =>
        _senders.TryGetValue(channelType, out var sender)
            ? sender
            : throw new NotImplementedException($"No sender is implemented for channel {channelType} yet.");
}
