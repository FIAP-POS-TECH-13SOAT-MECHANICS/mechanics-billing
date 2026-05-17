using Mechanics.Infra.Data.Models;

namespace Mechanics.Infra.Data.Repositories;

public interface IBudgetRepository : IRepository
{
    Task Upsert(BudgetModel budget, CancellationToken cancellationToken = default);
}
