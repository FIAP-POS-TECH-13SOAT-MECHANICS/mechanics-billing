using Mechanics.Domain.Base;

namespace Mechanics.Domain.WorkOrders;

public class BudgetNew : AbstractEntity
{
    public required Guid WorkOrderId { get; init; }
    public required Guid CustomerId { get; init; }
    public required Guid VehicleId { get; init; }
    public required decimal Total { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required BudgetStatus Status { get; init; }
    public required IReadOnlyList<BudgetNewItem> Items { get; init; }
}

public class BudgetNewItem
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required decimal UnitPrice { get; init; }
    public required int Quantity { get; init; }
    public required decimal Subtotal { get; init; }
}
