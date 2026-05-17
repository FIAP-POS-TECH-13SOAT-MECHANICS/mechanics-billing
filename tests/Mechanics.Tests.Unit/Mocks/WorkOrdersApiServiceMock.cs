using Mechanics.Application.WorkOrders.Responses;
using Mechanics.Application.WorkOrders.Services;
using Mechanics.Domain.Customers;
using Mechanics.Domain.WorkOrders;

namespace Mechanics.Tests.Unit.Mocks;

public class WorkOrdersApiServiceMock : IWorkOrdersApiService
{
    public WorkOrderResponse? WorkOrder { get; set; }
    public CustomerResponse? Customer { get; set; }

    public int GetWorkOrderByIdCalls { get; private set; }
    public int GetCustomerByIdCalls { get; private set; }

    public Task<WorkOrderResponse?> GetWorkOrderById(Guid id, CancellationToken cancellationToken)
    {
        GetWorkOrderByIdCalls++;
        return Task.FromResult(WorkOrder?.Id == id ? WorkOrder : null);
    }

    public Task<CustomerResponse?> GetCustomerById(Guid id, CancellationToken cancellationToken)
    {
        GetCustomerByIdCalls++;
        return Task.FromResult(Customer?.Id == id ? Customer : null);
    }

    public static WorkOrdersApiServiceMock CreateForBudgetEvent(Application.Budgets.Events.BudgetCreatedEvent budgetEvent)
    {
        var now = budgetEvent.OccurredAt.DateTime;

        return new WorkOrdersApiServiceMock
        {
            WorkOrder = new WorkOrderResponse
            {
                Id = budgetEvent.WorkOrderId,
                AccessKey = "12345678",
                Status = WorkOrderStatus.PendingApproval,
                CreationDate = now,
                LastUpdate = now,
                CustomerId = budgetEvent.CustomerId,
                VehicleId = budgetEvent.VehicleId,
                ReportedProblem = "Freio fazendo ruido",
                Observations = "Cliente aguardando aprovacao",
            },
            Customer = new CustomerResponse
            {
                Id = budgetEvent.CustomerId,
                Name = "Cliente Teste",
                Email = "cliente@test.local",
                Document = new PersonalDocumentResponse
                {
                    Type = DocumentType.Cpf,
                    Number = "63196372006",
                },
            },
        };
    }
}
