using Mechanics.Api.Controllers.Payments;
using Mechanics.Application.Budgets.Events;
using Mechanics.Application.Budgets.Services;
using Mechanics.Application.Payments;
using Mechanics.Application.Payments.Events;
using Mechanics.Application.Payments.Services;
using Mechanics.Application.WorkOrders.Responses;
using Mechanics.Domain.Customers;
using Mechanics.Domain.Payments;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Data.Models;
using Mechanics.Tests.Unit.Helpers;
using Mechanics.Tests.Unit.Mocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace Mechanics.Tests.Unit.Tests.Payments;

[TestClass]
[TestCategory("Payments")]
[TestCategory("Webhook")]
public class PaymentsWebhookControllerTests
{
    [TestMethod]
    public async Task It_ShouldProcessApprovedPaymentWebhook_AndTriggerBudgetEventAndEmail()
    {
        // Arrange
        const string mercadoPagoPaymentId = "158916459128";
        const string externalReference = "work-order-54468513";

        var workOrderId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var now = DateTime.Now;

        var paymentRepository = new PaymentRepositoryMock();
        await paymentRepository.Upsert(new PaymentModel
        {
            Id = Guid.NewGuid(),
            WorkOrderId = workOrderId,
            BudgetId = budgetId,
            Amount = 10,
            Status = PaymentStatus.Created,
            ExternalReference = externalReference,
            MercadoPagoPaymentId = mercadoPagoPaymentId,
            MercadoPagoPreferenceId = "3136685732-test-preference",
            CreationDate = now,
            UpdatedAt = now,
        });

        var budgetRepository = new BudgetRepositoryMock();
        await budgetRepository.Upsert(new BudgetModel
        {
            Id = budgetId,
            WorkOrderId = workOrderId,
            CustomerId = customerId,
            VehicleId = vehicleId,
            Total = 10,
            CreationDate = now,
            ExpiresAt = now.AddDays(3),
            Status = BudgetStatus.Sent,
            Items = [],
        });

        var workOrdersApi = new WorkOrdersApiServiceMock
        {
            WorkOrder = new WorkOrderResponse
            {
                Id = workOrderId,
                AccessKey = "54468513",
                Status = WorkOrderStatus.PendingApproval,
                CreationDate = now,
                LastUpdate = now,
                CustomerId = customerId,
                VehicleId = vehicleId,
            },
            Customer = new CustomerResponse
            {
                Id = customerId,
                Name = "Cliente Teste",
                Email = "cliente@test.local",
                Document = new PersonalDocumentResponse
                {
                    Type = DocumentType.Cpf,
                    Number = "63196372006",
                },
            },
        };

        var eventPublisher = new EventPublisherMock();
        var emailService = new EmailServiceMock();

        var paymentService = new PaymentAppService(
            paymentRepository,
            new ApprovedPaymentGateway(mercadoPagoPaymentId, externalReference),
            NullLogger<PaymentAppService>.Instance);

        var budgetService = new BudgetAppService(
            emailService,
            NullLogger<BudgetAppService>.Instance,
            workOrdersApi,
            budgetRepository);

        var controller = new PaymentsWebhookController(
            eventPublisher,
            paymentService,
            budgetService,
            workOrdersApi,
            emailService,
            NullLogger<PaymentsWebhookController>.Instance);

        using var payload = JsonDocument.Parse("""
                                               {
                                                 "action": "payment.created",
                                                 "data": { "id": "158916459128" },
                                                 "type": "payment"
                                               }
                                               """);

        // Act
        var result = await controller.PaymentWebhook(
            payload.RootElement,
            id: null,
            dataId: null,
            type: null,
            topic: null,
            CancellationToken.None);

        // Assert
        Assert.IsInstanceOfType<AcceptedResult>(result);

        Assert.IsNotNull(paymentRepository.LastPayment);
        Assert.AreEqual(PaymentStatus.Approved, paymentRepository.LastPayment.Status);
        Assert.AreEqual("accredited", paymentRepository.LastPayment.StatusDetail);
        Assert.IsNotNull(paymentRepository.LastPayment.LastWebhookReceivedAt);

        Assert.IsNotNull(budgetRepository.LastBudget);
        Assert.AreEqual(BudgetStatus.Approved, budgetRepository.LastBudget.Status);
        Assert.AreEqual("63196372006", budgetRepository.LastBudget.ApprovedByCustomerDocument);
        Assert.IsNotNull(budgetRepository.LastBudget.ApprovedAt);

        Assert.IsTrue(eventPublisher.PublishedMessages.OfType<PaymentApprovedEvent>().Any(e => e.WorkOrderId == workOrderId));
        Assert.IsTrue(eventPublisher.PublishedMessages.OfType<BudgetRevisedEvent>().Any(e => e.WorkOrderId == workOrderId && e.Approved));

        Assert.IsTrue(emailService.SendCustomerPaymentApprovedCalled);
        Assert.AreEqual("cliente@test.local", emailService.LastPaymentApprovedNotification?.CustomerEmail);
        Assert.AreEqual(10, emailService.LastPaymentApprovedNotification?.Amount);
    }

    private sealed class ApprovedPaymentGateway(string paymentId, string externalReference) : IPaymentGateway
    {
        public Task<PaymentGatewayResult> CreatePaymentAsync(CreatePaymentInput input, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<PaymentGatewayResult> GetPaymentAsync(string paymentIdFromWebhook, CancellationToken cancellationToken = default) =>
            Task.FromResult(new PaymentGatewayResult
            {
                PaymentId = paymentId,
                ExternalReference = externalReference,
                Status = "approved",
                StatusDetail = "accredited",
            });
    }
}
