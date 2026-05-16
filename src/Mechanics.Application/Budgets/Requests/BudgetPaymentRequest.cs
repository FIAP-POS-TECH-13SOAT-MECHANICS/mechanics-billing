namespace Mechanics.Application.Budgets.Requests;

public class BudgetPaymentRequest
{
    public string? PaymentMethodId { get; init; }
    public string? Token { get; init; }
    public int Installments { get; init; } = 1;
    public string? IssuerId { get; init; }
    public string? PayerEmail { get; init; }
}
