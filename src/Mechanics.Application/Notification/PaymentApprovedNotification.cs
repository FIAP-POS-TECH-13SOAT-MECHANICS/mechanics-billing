namespace Mechanics.Application.Notification;

public class PaymentApprovedNotification
{
    public required string CustomerEmail { get; init; }
    public required string CustomerName { get; init; }
    public required string PaymentId { get; init; }
    public required decimal Amount { get; init; }
}
