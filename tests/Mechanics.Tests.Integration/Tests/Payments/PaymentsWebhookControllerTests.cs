using Mechanics.Application.Payments;
using Mechanics.Domain.Payments;
using Mechanics.Infra.Data.Models;
using Mechanics.Infra.Data.Repositories;
using Mechanics.Tests.Integration.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Json;

namespace Mechanics.Tests.Integration.Tests.Payments;

[TestClass]
[TestCategory("Integration")]
[TestCategory("Payments")]
public class PaymentsWebhookControllerTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task It_ShouldAcceptMercadoPagoPaymentWebhook_AndUpdatePaymentStatus()
    {
        // Arrange
        const string mercadoPagoPaymentId = "158916459128";
        const string externalReference = "work-order-54468513";

        var paymentGateway = new FakePaymentGateway
        {
            Payment = new PaymentGatewayResult
            {
                PaymentId = mercadoPagoPaymentId,
                ExternalReference = externalReference,
                Status = "approved",
                StatusDetail = "accredited",
            },
        };

        var factory = TestProperties.Factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPaymentGateway>();
                services.AddSingleton<IPaymentGateway>(paymentGateway);
            }));

        var client = factory.CreateClient();
        var paymentId = await SeedPaymentAsync(factory.Services, externalReference);

        var payload = new
        {
            action = "payment.created",
            api_version = "v1",
            data = new { id = mercadoPagoPaymentId },
            date_created = "2021-11-01T02:02:02Z",
            id = mercadoPagoPaymentId,
            live_mode = false,
            type = "payment",
            user_id = 191754574,
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/billing/payments/webhook",
            payload,
            TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var paymentRepository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
        var payment = await paymentRepository.GetById(paymentId, TestContext.CancellationTokenSource.Token);

        Assert.IsNotNull(payment);
        Assert.AreEqual(mercadoPagoPaymentId, payment.MercadoPagoPaymentId);
        Assert.AreEqual(PaymentStatus.Approved, payment.Status);
        Assert.AreEqual("accredited", payment.StatusDetail);
        Assert.IsNotNull(payment.LastWebhookReceivedAt);
    }

    private static async Task<Guid> SeedPaymentAsync(IServiceProvider serviceProvider, string externalReference)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var paymentRepository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var workOrderId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var now = DateTime.Now;

        var payment = new PaymentModel
        {
            Id = Guid.NewGuid(),
            WorkOrderId = workOrderId,
            BudgetId = budgetId,
            Amount = 10,
            Status = PaymentStatus.Created,
            ExternalReference = externalReference,
            MercadoPagoPreferenceId = "3136685732-test-preference",
            CheckoutUrl = "https://www.mercadopago.com.br/checkout/v1/redirect",
            SandboxCheckoutUrl = "https://sandbox.mercadopago.com.br/checkout/v1/redirect",
            CreationDate = now,
            UpdatedAt = now,
        };

        await paymentRepository.Upsert(payment);

        return payment.Id;
    }

    private sealed class FakePaymentGateway : IPaymentGateway
    {
        public required PaymentGatewayResult Payment { get; init; }

        public Task<PaymentGatewayResult> CreatePaymentAsync(CreatePaymentInput input, CancellationToken cancellationToken = default) =>
            Task.FromResult(Payment);

        public Task<PaymentGatewayResult> GetPaymentAsync(string paymentId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Payment);
    }
}
