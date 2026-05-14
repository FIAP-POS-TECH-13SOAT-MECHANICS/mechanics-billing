namespace Mechanics.Application.Payments;

public class CreatePaymentInput
{
    public required Guid WorkOrderId { get; init; }
    public required Guid BudgetId { get; init; }
    public required decimal Amount { get; init; }
    public required string Description { get; init; }
    public required string PayerEmail { get; init; }
    public required string PaymentMethodId { get; init; }
    public string? Token { get; init; }
    public int Installments { get; init; } = 1;
    public string? IssuerId { get; init; }
}
