using Mechanics.Application.Notification;
using Mechanics.Application.Notification.Templates;
using Mechanics.Infra.Integrations.EmailSender;
using Microsoft.Extensions.Logging;

namespace Mechanics.Application.Notification.Services;

public class EmailService(ILogger<EmailService> logger, IEmailSenderService senderService) : IEmailService
{

    public async Task SendCustomerPaymentApproved(PaymentApprovedNotification notification, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending payment approved notification to '{EmailAddress}'", notification.CustomerEmail);

        var message = PaymentEmailTemplates.PaymentApproved(notification);
        await senderService.SendAsync(message, cancellationToken);

        logger.LogInformation("Payment approved notification sent to '{EmailAddress}'", notification.CustomerEmail);
    }
}
