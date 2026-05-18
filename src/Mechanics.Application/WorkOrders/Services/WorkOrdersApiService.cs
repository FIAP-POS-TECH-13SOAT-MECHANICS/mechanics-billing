using Mechanics.Application.WorkOrders.Responses;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mechanics.Application.WorkOrders.Services;

public class WorkOrdersApiService(HttpClient client) : IWorkOrdersApiService
{
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
        { Converters = { new JsonStringEnumConverter() } };

    public async Task<WorkOrderResponse?> GetWorkOrderById(Guid id, CancellationToken cancellationToken)
    {
        var response = await client.GetAsync($"work-orders/work-orders/{id}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<WorkOrderResponse>(_serializerOptions, cancellationToken);
    }

    public async Task<CustomerResponse?> GetCustomerById(Guid id, CancellationToken cancellationToken)
    {
        var response = await client.GetAsync($"work-orders/customers/{id}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CustomerResponse>(_serializerOptions, cancellationToken);
    }
}
