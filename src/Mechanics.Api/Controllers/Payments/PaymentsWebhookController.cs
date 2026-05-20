using Mechanics.Application.Notification;
using Mechanics.Application.Notification.Services;
using Mechanics.Application.Budgets.Events;
using Mechanics.Application.Budgets.Services;
using Mechanics.Application.Payments.Events;
using Mechanics.Application.Payments.Services;
using Mechanics.Application.WorkOrders.Services;
using Mechanics.Domain.Payments;
using Mechanics.Infra.Messaging.Publishers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Mechanics.Api.Controllers.Payments;

[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Route("payments/webhook")]
[AllowAnonymous]
public class PaymentsWebhookController(
    IEventPublisher eventPublisher,
    PaymentAppService paymentService,
    BudgetAppService budgetService,
    IWorkOrdersApiService workOrdersApiService,
    IEmailService emailService,
    ILogger<PaymentsWebhookController> logger)
    : ControllerBase
{
    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> PaymentWebhook(
        [FromBody] JsonElement payload,
        [FromQuery(Name = "id")] string? id,
        [FromQuery(Name = "data.id")] string? dataId,
        [FromQuery] string? type,
        [FromQuery] string? topic,
        CancellationToken cancellationToken)
    {
        var paymentId = dataId ?? id ?? TryReadPaymentId(payload);
        var eventType = type ?? topic ?? TryReadString(payload, "type");
        var action = TryReadString(payload, "action");
        Payment? payment = null;

        try
        {
            payment = await paymentService.ProcessMercadoPagoWebhookAsync(paymentId, eventType, action, payload, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to persist Mercado Pago webhook for payment {PaymentId}", paymentId);
        }

        if (payment is not null && payment.Status == PaymentStatus.Approved)
        {
            try
            {
                await eventPublisher.PublishAsync(new PaymentApprovedEvent
                {
                    WorkOrderId = payment.WorkOrderId,
                    PaymentId = payment.MercadoPagoPaymentId ?? paymentId,
                    Type = eventType,
                    Action = action,
                    Payload = payload,
                    PaidAt = DateTimeOffset.UtcNow,
                }, cancellationToken);

                var workOrderResponse = await workOrdersApiService.GetWorkOrderById(payment.WorkOrderId, cancellationToken);
                if (workOrderResponse is null)
                {
                    logger.LogWarning("Failed to send payment approved email because work order {WorkOrderId} was not found", payment.WorkOrderId);
                    return Accepted();
                }

                var customerResponse = await workOrdersApiService.GetCustomerById(workOrderResponse.CustomerId, cancellationToken);
                if (customerResponse is null)
                {
                    logger.LogWarning("Failed to send payment approved email because customer {CustomerId} was not found", workOrderResponse.CustomerId);
                    return Accepted();
                }

                await budgetService.MarkBudgetApprovedAfterPayment(
                    payment.BudgetId,
                    customerResponse.Document.Number,
                    "Payment approved by Mercado Pago webhook",
                    cancellationToken);

                await eventPublisher.PublishAsync(new BudgetRevisedEvent
                {
                    WorkOrderId = payment.WorkOrderId,
                    Approved = true,
                    Notes = "Payment approved by Mercado Pago webhook",
                    OccurredAt = DateTimeOffset.UtcNow,
                }, cancellationToken);

                await emailService.SendCustomerPaymentApproved(new PaymentApprovedNotification
                {
                    CustomerEmail = customerResponse.Email,
                    CustomerName = customerResponse.Name,
                    PaymentId = payment.MercadoPagoPaymentId ?? paymentId ?? payment.Id.ToString(),
                    Amount = payment.Amount,
                }, cancellationToken);


            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to publish payment processed event for Mercado Pago payment {PaymentId}", paymentId);
            }
        }

        logger.LogInformation(
            "Mercado Pago webhook received | {payment.id} | {payment.type} | {payment.action}",
            paymentId,
            eventType,
            action);

        return Accepted();
    }

    private static string? TryReadPaymentId(JsonElement payload)
    {
        if (payload.ValueKind != JsonValueKind.Object)
            return null;

        if (payload.TryGetProperty("data", out var data) &&
            data.ValueKind == JsonValueKind.Object &&
            data.TryGetProperty("id", out var dataId))
            return dataId.ToString();

        return TryReadString(payload, "id");
    }

    private static string? TryReadString(JsonElement payload, string propertyName)
    {
        if (payload.ValueKind == JsonValueKind.Object &&
            payload.TryGetProperty(propertyName, out var value))
            return value.ToString();

        return null;
    }
}
