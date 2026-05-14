using Mechanics.Application.Utils;
using Mechanics.Domain.Payments;
using Mechanics.Infra.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Mechanics.Application.Payments.Services;

public class PaymentAppService(
    AppDbContext dbContext,
    IPaymentGateway paymentGateway,
    ILogger<PaymentAppService> logger)
    : IAppService
{
    public async Task RegisterCreatedPreferenceAsync(
        Guid workOrderId,
        Guid budgetId,
        decimal amount,
        PaymentGatewayResult preference,
        CancellationToken cancellationToken = default)
    {
        var existingPayment = await dbContext.Payments
            .FirstOrDefaultAsync(payment =>
                payment.MercadoPagoPreferenceId == preference.PreferenceId ||
                payment.ExternalReference == preference.ExternalReference,
                cancellationToken);

        if (existingPayment is not null)
            return;

        var now = DateTime.Now;
        await dbContext.Payments.AddAsync(new Payment
        {
            WorkOrderId = workOrderId,
            BudgetId = budgetId,
            Amount = amount,
            Status = PaymentStatus.Created,
            MercadoPagoPreferenceId = preference.PreferenceId,
            CheckoutUrl = preference.InitPoint,
            SandboxCheckoutUrl = preference.SandboxInitPoint,
            ExternalReference = preference.ExternalReference,
            UpdatedAt = now,
        }, cancellationToken);
    }

    public async Task ProcessMercadoPagoWebhookAsync(
        string? paymentId,
        string? eventType,
        string? action,
        JsonElement payload,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(eventType, "payment", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(paymentId))
        {
            logger.LogInformation(
                "Mercado Pago webhook ignored for payment persistence | {payment.id} | {payment.type} | {payment.action}",
                paymentId,
                eventType,
                action);
            return;
        }

        var mercadoPagoPayment = await paymentGateway.GetPaymentAsync(paymentId, cancellationToken);
        Payment? payment = null;

        if (!string.IsNullOrWhiteSpace(mercadoPagoPayment.PaymentId))
            payment = await dbContext.Payments
                .FirstOrDefaultAsync(p => p.MercadoPagoPaymentId == mercadoPagoPayment.PaymentId, cancellationToken);

        if (payment is null && !string.IsNullOrWhiteSpace(mercadoPagoPayment.ExternalReference))
            payment = await dbContext.Payments
                .FirstOrDefaultAsync(p => p.ExternalReference == mercadoPagoPayment.ExternalReference, cancellationToken);

        if (payment is null)
        {
            logger.LogWarning(
                "Mercado Pago payment webhook received but no local payment was found | {payment.id} | {payment.external_reference}",
                mercadoPagoPayment.PaymentId,
                mercadoPagoPayment.ExternalReference);
            return;
        }

        payment.MercadoPagoPaymentId = mercadoPagoPayment.PaymentId ?? payment.MercadoPagoPaymentId;
        payment.Status = MapStatus(mercadoPagoPayment.Status);
        payment.StatusDetail = mercadoPagoPayment.StatusDetail;
        payment.UpdatedAt = DateTime.Now;
        payment.LastWebhookReceivedAt = DateTime.Now;

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Payment persisted from Mercado Pago webhook | {payment.id} | {work_order.id} | {payment.status} | {payment.status_detail}",
            payment.MercadoPagoPaymentId,
            payment.WorkOrderId,
            payment.Status,
            payment.StatusDetail);
    }

    private static PaymentStatus MapStatus(string? mercadoPagoStatus) =>
        mercadoPagoStatus?.ToLowerInvariant() switch
        {
            "approved" => PaymentStatus.Approved,
            "authorized" => PaymentStatus.Approved,
            "pending" => PaymentStatus.Processing,
            "in_process" => PaymentStatus.Processing,
            "in_mediation" => PaymentStatus.Processing,
            "rejected" => PaymentStatus.Rejected,
            "cancelled" => PaymentStatus.Cancelled,
            "refunded" => PaymentStatus.Refunded,
            "charged_back" => PaymentStatus.Refunded,
            _ => PaymentStatus.Processing,
        };
}
