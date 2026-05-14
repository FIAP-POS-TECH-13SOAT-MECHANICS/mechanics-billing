namespace Mechanics.Application.Payments.Events;

public class PaymentCreatedEvent
{
    public required Guid WorkOrderId { get; init; }
    public required Guid BudgetId { get; init; }
    public required decimal Amount { get; init; }
    public string? PaymentId { get; init; }
    public string? PreferenceId { get; init; }
    public string? InitPoint { get; init; }
    public string? SandboxInitPoint { get; init; }
    public string? Status { get; init; }
    public string? StatusDetail { get; init; }
    public required string ExternalReference { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
