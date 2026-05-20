using Mechanics.Application.WorkOrders.Responses;

namespace Mechanics.Application.WorkOrders.Services;

public interface IWorkOrdersApiService
{
    Task<WorkOrderResponse?> GetWorkOrderById(Guid id, CancellationToken cancellationToken);
    Task<CustomerResponse?> GetCustomerById(Guid id, CancellationToken cancellationToken);
}
