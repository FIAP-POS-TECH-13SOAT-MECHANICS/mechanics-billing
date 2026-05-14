namespace Mechanics.Domain.Payments;

public enum PaymentStatus
{
    Created = 1,
    Processing = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5,
    Refunded = 6,
    Failed = 7,
}
