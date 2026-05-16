using Mechanics.Infra.Data.Models;
using Mechanics.Infra.Data.Repositories;

namespace Mechanics.Tests.Unit.Mocks;

public class BudgetRepositoryMock : IBudgetRepository
{
    public BudgetModel? LastBudget { get; private set; }
    public int UpsertCalls { get; private set; }

    public Task Upsert(BudgetModel budget, CancellationToken cancellationToken = default)
    {
        LastBudget = budget;
        UpsertCalls++;
        return Task.CompletedTask;
    }
}
