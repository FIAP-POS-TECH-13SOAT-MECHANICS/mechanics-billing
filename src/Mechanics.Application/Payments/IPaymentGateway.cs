namespace Mechanics.Application.Payments;

public interface IPaymentGateway
{
    Task<PaymentGatewayResult> CreatePaymentAsync(CreatePaymentInput input, CancellationToken cancellationToken = default);
    Task<PaymentGatewayResult> GetPaymentAsync(string paymentId, CancellationToken cancellationToken = default);
}
