using Mechanics.Domain.Base;

namespace Mechanics.Domain.Payments;

public class Payment : AbstractEntity
{
    public required Guid WorkOrderId { get; init; }
    public required Guid BudgetId { get; init; }
    public string? MercadoPagoPaymentId { get; set; }
    public string? MercadoPagoPreferenceId { get; set; }
    public required string ExternalReference { get; init; }
    public required decimal Amount { get; init; }
    public required PaymentStatus Status { get; set; }
    public string? StatusDetail { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? SandboxCheckoutUrl { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastWebhookReceivedAt { get; set; }
}
