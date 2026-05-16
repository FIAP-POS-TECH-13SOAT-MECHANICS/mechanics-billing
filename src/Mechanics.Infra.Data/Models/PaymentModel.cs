using Amazon.DynamoDBv2.DataModel;
using Mechanics.Domain.Payments;
using Mechanics.Infra.Data.TypeConverters;

namespace Mechanics.Infra.Data.Models;

[DynamoDBTable(nameof(PaymentModel), LowerCamelCaseProperties = true)]
public class PaymentModel
{
    [DynamoDBHashKey(typeof(GuidTypeConverter))]
    public required Guid Id { get; init; }

    [DynamoDBGlobalSecondaryIndexHashKey]
    public required Guid WorkOrderId { get; init; }

    [DynamoDBProperty(typeof(GuidTypeConverter))]
    public required Guid BudgetId { get; init; }

    [DynamoDBGlobalSecondaryIndexHashKey]
    public string? MercadoPagoPaymentId { get; set; }

    [DynamoDBProperty]
    public string? MercadoPagoPreferenceId { get; set; }

    [DynamoDBGlobalSecondaryIndexHashKey]
    public required string ExternalReference { get; init; }

    [DynamoDBProperty]
    public required decimal Amount { get; init; }

    [DynamoDBProperty(typeof(EnumConverter<PaymentStatus>))]
    public PaymentStatus Status { get; init; }

    [DynamoDBProperty]
    public string? StatusDetail { get; set; }

    [DynamoDBProperty]
    public string? CheckoutUrl { get; set; }

    [DynamoDBProperty]
    public string? SandboxCheckoutUrl { get; set; }

    [DynamoDBProperty]
    public required DateTime CreationDate { get; init; }

    [DynamoDBProperty]
    public required DateTime UpdatedAt { get; set; }

    [DynamoDBProperty]
    public DateTime? LastWebhookReceivedAt { get; set; }

    public Payment ToModel()
    {
        return new Payment
        {
            Id = Id,
            WorkOrderId = WorkOrderId,
            BudgetId = BudgetId,
            MercadoPagoPaymentId = MercadoPagoPaymentId,
            MercadoPagoPreferenceId = MercadoPagoPreferenceId,
            ExternalReference = ExternalReference,
            Amount = Amount,
            Status = Status,
            StatusDetail = StatusDetail,
            CheckoutUrl = CheckoutUrl,
            SandboxCheckoutUrl = SandboxCheckoutUrl,
            CreationDate = CreationDate,
            UpdatedAt = UpdatedAt,
            LastWebhookReceivedAt = LastWebhookReceivedAt,
        };
    }

    public static PaymentModel FromModel(Payment payment)
    {
        return new PaymentModel
        {
            Id = payment.Id,
            WorkOrderId = payment.WorkOrderId,
            BudgetId = payment.BudgetId,
            MercadoPagoPaymentId = payment.MercadoPagoPaymentId,
            MercadoPagoPreferenceId = payment.MercadoPagoPreferenceId,
            ExternalReference = payment.ExternalReference,
            Amount = payment.Amount,
            Status = payment.Status,
            StatusDetail = payment.StatusDetail,
            CheckoutUrl = payment.CheckoutUrl,
            SandboxCheckoutUrl = payment.SandboxCheckoutUrl,
            CreationDate = payment.CreationDate,
            UpdatedAt = payment.UpdatedAt,
            LastWebhookReceivedAt = payment.LastWebhookReceivedAt,
        };
    }
}
