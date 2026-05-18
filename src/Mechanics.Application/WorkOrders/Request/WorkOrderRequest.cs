using Mechanics.Application.WorkOrders.Responses;
using Mechanics.Domain.WorkOrders;

namespace Mechanics.Application.WorkOrders.Request;

public class WorkOrderRequest
{
    public required Guid Id { get; init; }
    public required string AccessKey { get; init; }
    public required WorkOrderStatus Status { get; init; }
    public required DateTime CreationDate { get; init; }
    public DateTime? LastUpdate { get; init; }
    public Guid CustomerId { get; init; }
    public Guid VehicleId { get; init; }

    public string? ReportedProblem { get; init; }
    public string? Observations { get; init; }

    public IEnumerable<WorkOrderProductResponse>? Products { get; init; }
    public IEnumerable<Guid>? ServiceCatalogIds { get; init; }
}
