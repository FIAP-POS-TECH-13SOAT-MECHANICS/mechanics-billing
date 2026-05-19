using Mechanics.Infra.Data.Models;
using Mechanics.Infra.Data.Repositories;

namespace Mechanics.Tests.Unit.Mocks;

public class PaymentRepositoryMock : IPaymentRepository
{
    public PaymentModel? LastPayment { get; private set; }
    public int UpsertCalls { get; private set; }

    public Task<PaymentModel?> GetById(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(LastPayment?.Id == id ? LastPayment : null);

    public Task<PaymentModel?> GetByExternalReference(string externalReference, CancellationToken cancellationToken = default) =>
        Task.FromResult(LastPayment?.ExternalReference == externalReference ? LastPayment : null);

    public Task<PaymentModel?> GetByMercadoPagoPaymentId(string mercadoPagoPaymentId, CancellationToken cancellationToken = default) =>
        Task.FromResult(LastPayment?.MercadoPagoPaymentId == mercadoPagoPaymentId ? LastPayment : null);

    public Task<IReadOnlyList<PaymentModel>> GetByWorkOrderId(Guid workOrderId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PaymentModel> payments = LastPayment?.WorkOrderId == workOrderId
            ? [LastPayment]
            : [];

        return Task.FromResult(payments);
    }

    public Task Upsert(PaymentModel payment, CancellationToken cancellationToken = default)
    {
        LastPayment = payment;
        UpsertCalls++;
        return Task.CompletedTask;
    }
}
