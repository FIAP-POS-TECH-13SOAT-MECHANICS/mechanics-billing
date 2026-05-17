using Mechanics.Infra.Data.Models;

namespace Mechanics.Infra.Data.Repositories;

public interface IPaymentRepository : IRepository
{
    Task<PaymentModel?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<PaymentModel?> GetByExternalReference(string externalReference, CancellationToken cancellationToken = default);
    Task<PaymentModel?> GetByMercadoPagoPaymentId(string mercadoPagoPaymentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentModel>> GetByWorkOrderId(Guid workOrderId, CancellationToken cancellationToken = default);
    Task Upsert(PaymentModel payment, CancellationToken cancellationToken = default);
}
