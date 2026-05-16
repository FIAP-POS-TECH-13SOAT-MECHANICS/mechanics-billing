using System.Text.Json;

namespace Mechanics.Application.Payments.Events;

public class PaymentApprovedEvent
{
    public string? PaymentId { get; init; }
    public string? Type { get; init; }
    public string? Action { get; init; }
    public JsonElement Payload { get; init; }
    public required Guid WorkOrderId { get; init; }
    public required DateTimeOffset PaidAt { get; init; }
}
