using Amazon.DynamoDBv2.DataModel;
using Mechanics.Infra.Data.Models;
using Mechanics.Infra.Data.Options;
using Microsoft.Extensions.Options;

namespace Mechanics.Infra.Data.Repositories;

public class PaymentRepository(IDynamoDBContext context, IOptions<TableNames> options) : IPaymentRepository
{
    private readonly string _tableName = options.Value.Payments;

    public async Task<PaymentModel?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.LoadAsync<PaymentModel>(id, new LoadConfig { OverrideTableName = _tableName }, cancellationToken);
    }

    public async Task<PaymentModel?> GetByExternalReference(string externalReference, CancellationToken cancellationToken = default)
    {
        var search = context.QueryAsync<PaymentModel>(QueryConditional.HashKeyEqualTo("externalReference", externalReference),
            new QueryConfig { OverrideTableName = _tableName, IndexName = "externalReference-index" });

        var results = await search.GetRemainingAsync(cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<PaymentModel?> GetByMercadoPagoPaymentId(string mercadoPagoPaymentId,
        CancellationToken cancellationToken = default)
    {
        var search = context.QueryAsync<PaymentModel>(QueryConditional.HashKeyEqualTo("mercadoPagoPaymentId", mercadoPagoPaymentId),
            new QueryConfig { OverrideTableName = _tableName, IndexName = "mercadoPagoPaymentId-index" });

        var results = await search.GetRemainingAsync(cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<IReadOnlyList<PaymentModel>> GetByWorkOrderId(Guid workOrderId, CancellationToken cancellationToken = default)
    {
        var search = context.QueryAsync<PaymentModel>(QueryConditional.HashKeyEqualTo("workOrderId", workOrderId.ToString()),
            new QueryConfig { OverrideTableName = _tableName, IndexName = "workOrderId-index" });

        return await search.GetRemainingAsync(cancellationToken);
    }

    public async Task Upsert(PaymentModel payment, CancellationToken cancellationToken = default)
    {
        await context.SaveAsync(payment, new SaveConfig { OverrideTableName = _tableName }, cancellationToken);
    }
}
