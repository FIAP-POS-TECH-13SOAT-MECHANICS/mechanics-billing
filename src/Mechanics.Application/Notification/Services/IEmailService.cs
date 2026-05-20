using Mechanics.Application.WorkOrders.Request;
using Mechanics.Domain.WorkOrders;

namespace Mechanics.Application.Notification.Services;

public interface IEmailService
{
    Task SendCustomerPaymentApproved(PaymentApprovedNotification notification, CancellationToken cancellationToken = default);

    Task SendWorkOrderPendingApproval(CustomerRequest customer, WorkOrderRequest workOrder, Budget budget,
        CancellationToken cancellationToken = default);
}
