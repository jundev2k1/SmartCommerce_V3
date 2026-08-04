using SmartEcommerce.BuildingBlock.Infrastructure.Mail.Models;

namespace SmartEcommerce.BuildingBlock.Infrastructure.Mail.Abstractions;

public interface IEmailSender
{
    Task<EmailResult> SendAsync(
        EmailMessage message,
        CancellationToken ct = default);
}
