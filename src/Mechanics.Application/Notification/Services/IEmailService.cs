using Mechanics.Application.Notification;

namespace Mechanics.Application.Notification.Services;

public interface IEmailService
{
    Task SendCustomerPaymentApproved(PaymentApprovedNotification notification, CancellationToken cancellationToken = default);
}
