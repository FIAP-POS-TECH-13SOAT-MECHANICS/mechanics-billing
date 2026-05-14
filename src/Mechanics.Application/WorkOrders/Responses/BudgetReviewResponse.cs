namespace Mechanics.Application.WorkOrders.Responses;

public class BudgetReviewResponse
{
    public required Guid WorkOrderId { get; init; }
    public required Guid BudgetId { get; init; }
    public PaymentReviewResponse? Payment { get; init; }
}

public class PaymentReviewResponse
{
    public string? PaymentId { get; init; }
    public string? PreferenceId { get; init; }
    public string? InitPoint { get; init; }
    public string? SandboxInitPoint { get; init; }
    public string? Status { get; init; }
    public string? StatusDetail { get; init; }
    public required string ExternalReference { get; init; }
}
