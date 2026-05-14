using Mechanics.Application.Payments;

namespace Mechanics.Tests.Unit.Helpers;

public class NullPaymentGateway : IPaymentGateway
{
    public Task<PaymentGatewayResult> CreatePaymentAsync(CreatePaymentInput input, CancellationToken cancellationToken = default) =>
        Task.FromResult(new PaymentGatewayResult
        {
            PreferenceId = "test-preference-id",
            InitPoint = "https://www.mercadopago.com.br/checkout/v1/redirect?pref_id=test-preference-id",
            SandboxInitPoint = "https://sandbox.mercadopago.com.br/checkout/v1/redirect?pref_id=test-preference-id",
            ExternalReference = $"{input.WorkOrderId}:{input.BudgetId}",
        });

    public Task<PaymentGatewayResult> GetPaymentAsync(string paymentId, CancellationToken cancellationToken = default) =>
        Task.FromResult(new PaymentGatewayResult
        {
            PaymentId = paymentId,
            Status = "approved",
            StatusDetail = "accredited",
            ExternalReference = string.Empty,
        });
}
