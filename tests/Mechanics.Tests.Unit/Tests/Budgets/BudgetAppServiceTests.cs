using Mechanics.Application.Budgets.Consumers;
using Mechanics.Application.Budgets.Services;
using Mechanics.Domain.WorkOrders;
using Mechanics.Tests.Unit.Mocks;
using Microsoft.Extensions.Logging.Abstractions;

namespace Mechanics.Tests.Unit.Tests.Budgets;

[TestClass]
[TestCategory("Budget")]
public class BudgetAppServiceTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod("deve criar orçamento a partir da mensagem recebida via SQS")]
    public async Task It_ShouldCreateBudget_WhenBudgetCreatedEventIsConsumed()
    {
        // Arrange
        var budgetCreatedEvent = BudgetMocks.CreateBudgetCreatedEvent();
        var emailMock = new EmailServiceMock();
        var workOrdersApiMock = WorkOrdersApiServiceMock.CreateForBudgetEvent(budgetCreatedEvent);
        var budgetRepositoryMock = new BudgetRepositoryMock();

        var appService = new BudgetAppService(
            emailMock,
            NullLogger<BudgetAppService>.Instance,
            workOrdersApiMock,
            budgetRepositoryMock);
        var consumer = new BudgetCreatedEventConsumer(appService);

        // Act
        await consumer.ConsumeAsync(budgetCreatedEvent, TestContext.CancellationTokenSource.Token);

        // Assert
        var persistedBudget = budgetRepositoryMock.LastBudget;
        Assert.IsNotNull(persistedBudget, "Budget deve ser persistido a partir da mensagem");
        Assert.AreEqual(1, budgetRepositoryMock.UpsertCalls, "Mensagem deve persistir apenas um budget");
        Assert.AreEqual(budgetCreatedEvent.EventId, persistedBudget.Id);
        Assert.AreEqual(budgetCreatedEvent.WorkOrderId, persistedBudget.WorkOrderId);
        Assert.AreEqual(budgetCreatedEvent.CustomerId, persistedBudget.CustomerId);
        Assert.AreEqual(budgetCreatedEvent.VehicleId, persistedBudget.VehicleId);
        Assert.AreEqual(budgetCreatedEvent.Total, persistedBudget.Total);
        Assert.AreEqual(BudgetStatus.Sent, persistedBudget.Status);
        Assert.AreEqual(budgetCreatedEvent.ExpiresAt.DateTime, persistedBudget.ExpiresAt);
        Assert.HasCount(budgetCreatedEvent.Items.Count, persistedBudget.Items);

        Assert.AreEqual(1, workOrdersApiMock.GetWorkOrderByIdCalls);
        Assert.AreEqual(1, workOrdersApiMock.GetCustomerByIdCalls);
        Assert.IsTrue(emailMock.SendWorkOrderPendingApprovalCalled, "E-mail de aprovação deve ser enviado");
        Assert.AreEqual(budgetCreatedEvent.EventId, emailMock.LastBudgetId);
        Assert.AreEqual(budgetCreatedEvent.Total, emailMock.LastBudgetTotal);
    }

    [TestMethod("deve persistir orçamento e ignorar e-mail quando dados externos não forem encontrados")]
    public async Task It_ShouldPersistBudgetAndSkipEmail_WhenExternalDataIsMissing()
    {
        // Arrange
        var budgetCreatedEvent = BudgetMocks.CreateBudgetCreatedEvent();
        var emailMock = new EmailServiceMock();
        var workOrdersApiMock = new WorkOrdersApiServiceMock();
        var budgetRepositoryMock = new BudgetRepositoryMock();

        var appService = new BudgetAppService(
            emailMock,
            NullLogger<BudgetAppService>.Instance,
            workOrdersApiMock,
            budgetRepositoryMock);

        // Act
        await appService.CreateAndSendBudget(budgetCreatedEvent, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsNotNull(budgetRepositoryMock.LastBudget, "Budget deve ser persistido mesmo sem dados externos");
        Assert.AreEqual(1, budgetRepositoryMock.UpsertCalls);
        Assert.AreEqual(1, workOrdersApiMock.GetWorkOrderByIdCalls);
        Assert.AreEqual(1, workOrdersApiMock.GetCustomerByIdCalls);
        Assert.IsFalse(emailMock.SendWorkOrderPendingApprovalCalled, "E-mail deve ser ignorado sem WorkOrder ou Customer");
    }
}
