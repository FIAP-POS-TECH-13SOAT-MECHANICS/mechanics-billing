namespace Mechanics.Application.Budgets.Requests;

/// <summary>
///     Requisição para aprovação de budget pelo cliente.
/// </summary>
public class BudgetReviewRequest
{
    public required string AccessKey { get; init; }

    /// <summary>
    /// Comentário opcional informado pelo cliente (para aprovar ou rejeitar).
    /// </summary>
    public string? Description { get; init; }
    public BudgetPaymentRequest? Payment { get; init; }
}
