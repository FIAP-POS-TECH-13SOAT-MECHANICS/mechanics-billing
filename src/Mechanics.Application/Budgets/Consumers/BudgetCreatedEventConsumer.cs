using Mechanics.Application.Budgets.Events;
using Mechanics.Application.Budgets.Services;
using Mechanics.Infra.Messaging.Consumers;

namespace Mechanics.Application.Budgets.Consumers;

public class BudgetCreatedEventConsumer(BudgetAppService budgetAppService) : IEventConsumer<BudgetCreatedEvent>
{
    public Task ConsumeAsync(BudgetCreatedEvent message, CancellationToken cancellationToken = default)
    {
        return budgetAppService.CreateAndSendBudget(message, cancellationToken);
    }
}
