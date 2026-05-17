using Mechanics.Application.Payments.Events;
using Mechanics.Application.Payments.Services;
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
    ILogger<PaymentsWebhookController> logger)
    : ControllerBase
{
    [HttpPost("mercado-pago")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> MercadoPago(
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

        try
        {
            await paymentService.ProcessMercadoPagoWebhookAsync(paymentId, eventType, action, payload, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to persist Mercado Pago webhook for payment {PaymentId}", paymentId);
        }

        try
        {
            await eventPublisher.PublishAsync(new PaymentApprovedEvent
            {
                WorkOrderId = Guid.Empty,
                PaymentId = paymentId,
                Type = eventType,
                Action = action,
                Payload = payload,
                PaidAt = DateTimeOffset.UtcNow,
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to publish payment processed event for Mercado Pago payment {PaymentId}", paymentId);
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
