using System.Text.Json;

namespace Mechanics.Application.Payments.Events;

public class PaymentProcessedEvent
{
    public string? PaymentId { get; init; }
    public string? Type { get; init; }
    public string? Action { get; init; }
    public JsonElement Payload { get; init; }
    public DateTime ProcessedAt { get; init; } = DateTime.UtcNow;
}
