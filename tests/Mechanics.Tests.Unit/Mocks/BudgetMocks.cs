using Mechanics.Application.Budgets.Events;

namespace Mechanics.Tests.Unit.Mocks;

public static class BudgetMocks
{
    public static BudgetCreatedEvent CreateBudgetCreatedEvent(
        Guid? eventId = null,
        Guid? workOrderId = null,
        Guid? customerId = null,
        Guid? vehicleId = null)
    {
        var occurredAt = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);

        return new BudgetCreatedEvent
        {
            EventId = eventId ?? Guid.NewGuid(),
            OccurredAt = occurredAt,
            WorkOrderId = workOrderId ?? Guid.NewGuid(),
            CustomerId = customerId ?? Guid.NewGuid(),
            VehicleId = vehicleId ?? Guid.NewGuid(),
            Total = 350m,
            ExpiresAt = occurredAt.AddDays(3),
            Items =
            [
                new BudgetItem
                {
                    Id = Guid.NewGuid(),
                    Name = "Diagnostico",
                    UnitPrice = 150m,
                    Quantity = 1,
                    Subtotal = 150m,
                },
                new BudgetItem
                {
                    Id = Guid.NewGuid(),
                    Name = "Pastilha de freio",
                    UnitPrice = 100m,
                    Quantity = 2,
                    Subtotal = 200m,
                },
            ],
        };
    }
}
