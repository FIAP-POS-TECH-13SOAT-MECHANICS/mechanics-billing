using Amazon.DynamoDBv2.DataModel;
using Mechanics.Infra.Data.Models;
using Mechanics.Infra.Data.Options;
using Microsoft.Extensions.Options;

namespace Mechanics.Infra.Data.Repositories;

public class BudgetRepository(IDynamoDBContext context, IOptions<TableNames> options) : IBudgetRepository
{
    private readonly string _tableName = options.Value.Budgets;

    public async Task Upsert(BudgetModel budget, CancellationToken cancellationToken = default)
    {
        await context.SaveAsync(budget, new SaveConfig { OverrideTableName = _tableName }, cancellationToken);
    }
}
