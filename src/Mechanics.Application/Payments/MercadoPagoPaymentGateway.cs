using Mechanics.Domain.Base.Exceptions;
using MercadoPago.Client;
using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Error;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mechanics.Application.Payments;

public class MercadoPagoPaymentGateway(
    IOptions<MercadoPagoOptions> options,
    ILogger<MercadoPagoPaymentGateway> logger)
    : IPaymentGateway
{
    private readonly MercadoPagoOptions _options = options.Value;

    public async Task<PaymentGatewayResult> CreatePaymentAsync(CreatePaymentInput input, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
            throw new BusinessException("Mercado Pago payment gateway is disabled.");

        if (string.IsNullOrWhiteSpace(_options.AccessToken))
            throw new BusinessException("Mercado Pago access token is not configured.");

        var externalReference = $"{input.WorkOrderId}:{input.BudgetId}";
        logger.LogInformation(
            "Creating Mercado Pago Checkout Pro preference | {work_order.id} | {budget.id} | {payment.amount} | {payment.payer_email} | {payment.notification_url_configured} | {mercado_pago.access_token_prefix}",
            input.WorkOrderId,
            input.BudgetId,
            input.Amount,
            input.PayerEmail,
            !string.IsNullOrWhiteSpace(_options.NotificationUrl),
            GetAccessTokenPrefix(_options.AccessToken));

        var request = new PreferenceRequest
        {
            ExternalReference = externalReference,
            NotificationUrl = _options.NotificationUrl,
            StatementDescriptor = _options.StatementDescriptor,
            Payer = new PreferencePayerRequest
            {
                Email = input.PayerEmail,
            },
            Items =
            [
                new PreferenceItemRequest
                {
                    Id = input.BudgetId.ToString(),
                    Title = input.Description,
                    Description = $"Work order {input.WorkOrderId}",
                    Quantity = 1,
                    CurrencyId = "BRL",
                    UnitPrice = input.Amount,
                },
            ],
        };

        var requestOptions = new RequestOptions
        {
            AccessToken = _options.AccessToken,
        };

        try
        {
            var preference = await new PreferenceClient().CreateAsync(request, requestOptions, cancellationToken);

            logger.LogInformation(
                "Mercado Pago Checkout Pro preference created | {work_order.id} | {budget.id} | {preference.id} | {preference.external_reference}",
                input.WorkOrderId,
                input.BudgetId,
                preference.Id,
                preference.ExternalReference ?? externalReference);

            return new PaymentGatewayResult
            {
                PreferenceId = preference.Id,
                InitPoint = preference.InitPoint,
                SandboxInitPoint = preference.SandboxInitPoint,
                ExternalReference = preference.ExternalReference ?? externalReference,
            };
        }
        catch (MercadoPagoApiException ex)
        {
            logger.LogError(
                ex,
                "Mercado Pago Checkout Pro preference creation failed | {work_order.id} | {budget.id} | {payment.amount} | {payment.payer_email} | {payment.notification_url_configured} | {mercado_pago.access_token_prefix}",
                input.WorkOrderId,
                input.BudgetId,
                input.Amount,
                input.PayerEmail,
                !string.IsNullOrWhiteSpace(_options.NotificationUrl),
                GetAccessTokenPrefix(_options.AccessToken));
            throw;
        }
    }

    public async Task<PaymentGatewayResult> GetPaymentAsync(string paymentId, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
            throw new BusinessException("Mercado Pago payment gateway is disabled.");

        if (string.IsNullOrWhiteSpace(_options.AccessToken))
            throw new BusinessException("Mercado Pago access token is not configured.");

        var requestOptions = new RequestOptions
        {
            AccessToken = _options.AccessToken,
        };

        var payment = await new PaymentClient().GetAsync(long.Parse(paymentId), requestOptions, cancellationToken);

        return new PaymentGatewayResult
        {
            PaymentId = payment.Id?.ToString(),
            Status = payment.Status,
            StatusDetail = payment.StatusDetail,
            ExternalReference = payment.ExternalReference ?? string.Empty,
        };
    }

    private static string GetAccessTokenPrefix(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return "<empty>";

        var visibleLength = Math.Min(accessToken.Length, 8);
        return $"{accessToken[..visibleLength]}...";
    }
}
