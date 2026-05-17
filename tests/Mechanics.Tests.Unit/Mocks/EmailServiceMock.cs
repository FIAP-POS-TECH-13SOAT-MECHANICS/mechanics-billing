using Mechanics.Application.Notification;
using Mechanics.Application.Notification.Services;

namespace Mechanics.Tests.Unit.Mocks;

public class EmailServiceMock : IEmailService
{
    public bool SendCustomerPaymentApprovedCalled { get; private set; }
    public PaymentApprovedNotification? LastPaymentApprovedNotification { get; private set; }

    public Task SendCustomerPaymentApproved(PaymentApprovedNotification notification, CancellationToken cancellationToken = default)
    {
        SendCustomerPaymentApprovedCalled = true;
        LastPaymentApprovedNotification = notification;
        return Task.CompletedTask;
    }
}
