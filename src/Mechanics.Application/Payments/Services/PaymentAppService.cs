using Mechanics.Application.Utils;
using Mechanics.Domain.Payments;
using Mechanics.Infra.Data.Models;
using Mechanics.Infra.Data.Repositories;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Mechanics.Application.Payments.Services;

public class PaymentAppService(
    IPaymentRepository paymentRepository,
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
        var existingPayment = await paymentRepository.GetByExternalReference(preference.ExternalReference, cancellationToken);

        if (existingPayment is not null)
            return;

        var now = DateTime.Now;
        await paymentRepository.Upsert(new PaymentModel
        {
            Id = Guid.NewGuid(),
            WorkOrderId = workOrderId,
            BudgetId = budgetId,
            Amount = amount,
            Status = PaymentStatus.Created,
            MercadoPagoPreferenceId = preference.PreferenceId,
            CheckoutUrl = preference.InitPoint,
            SandboxCheckoutUrl = preference.SandboxInitPoint,
            ExternalReference = preference.ExternalReference,
            CreationDate = now,
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
            payment =
                (await paymentRepository.GetByMercadoPagoPaymentId(mercadoPagoPayment.PaymentId, cancellationToken))?.ToModel();

        if (payment is null && !string.IsNullOrWhiteSpace(mercadoPagoPayment.ExternalReference))
            payment = (await paymentRepository.GetByExternalReference(mercadoPagoPayment.ExternalReference, cancellationToken))
                ?.ToModel();

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

        await paymentRepository.Upsert(PaymentModel.FromModel(payment), cancellationToken);

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
            "approved" or "authorized" => PaymentStatus.Approved,
            "pending" or "in_process" or "in_mediation" => PaymentStatus.Processing,
            "rejected" => PaymentStatus.Rejected,
            "cancelled" => PaymentStatus.Cancelled,
            "refunded" or "charged_back" => PaymentStatus.Refunded,
            _ => PaymentStatus.Processing,
        };
}
