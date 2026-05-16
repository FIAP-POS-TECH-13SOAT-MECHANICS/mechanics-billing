using Mechanics.Application.WorkOrders.Responses;

namespace Mechanics.Application.WorkOrders;

public interface IWorkOrdersApiService
{
    Task<WorkOrderResponse?> GetWorkOrderById(Guid id, CancellationToken cancellationToken);
    Task<CustomerResponse?> GetCustomerById(Guid id, CancellationToken cancellationToken);
}
