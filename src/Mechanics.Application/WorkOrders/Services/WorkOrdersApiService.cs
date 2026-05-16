using Mechanics.Application.WorkOrders.Responses;
using System.Net;
using System.Net.Http.Json;

namespace Mechanics.Application.WorkOrders.Services;

public class WorkOrdersApiService(HttpClient client) : IWorkOrdersApiService
{
    public async Task<WorkOrderResponse?> GetWorkOrderById(Guid id, CancellationToken cancellationToken)
    {
        var response = await client.GetAsync($"work-orders/work-orders/{id}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<WorkOrderResponse>(cancellationToken: cancellationToken);
    }

    public async Task<CustomerResponse?> GetCustomerById(Guid id, CancellationToken cancellationToken)
    {
        var response = await client.GetAsync($"work-orders/customers/{id}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CustomerResponse>(cancellationToken: cancellationToken);
    }
}
