using Mechanics.Application.Notification;
using Mechanics.Application.Notification.Services;
using Mechanics.Application.WorkOrders.Request;
using Mechanics.Domain.WorkOrders;

namespace Mechanics.Tests.Unit.Mocks;

public class EmailServiceMock : IEmailService
{
    public bool SendWorkOrderPendingApprovalCalled { get; private set; }
    public Budget? LastBudget { get; private set; }
    public Guid? LastBudgetId => LastBudget?.Id;
    public decimal? LastBudgetTotal => LastBudget?.Total;
    public bool SendCustomerPaymentApprovedCalled { get; private set; }
    public PaymentApprovedNotification? LastPaymentApprovedNotification { get; private set; }

    public Task SendCustomerPaymentApproved(PaymentApprovedNotification notification, CancellationToken cancellationToken = default)
    {
        SendCustomerPaymentApprovedCalled = true;
        LastPaymentApprovedNotification = notification;
        return Task.CompletedTask;
    }

    public Task SendWorkOrderPendingApproval(CustomerRequest customer, WorkOrderRequest workOrder, Budget budget,
        CancellationToken cancellationToken = default)
    {
        SendWorkOrderPendingApprovalCalled = true;
        LastBudget = budget;
        return Task.CompletedTask;
    }
}
