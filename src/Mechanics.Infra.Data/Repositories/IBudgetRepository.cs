using Mechanics.Infra.Data.Models;

namespace Mechanics.Infra.Data.Repositories;

public interface IBudgetRepository : IRepository
{
    Task<BudgetModel?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task Upsert(BudgetModel budget, CancellationToken cancellationToken = default);
}
