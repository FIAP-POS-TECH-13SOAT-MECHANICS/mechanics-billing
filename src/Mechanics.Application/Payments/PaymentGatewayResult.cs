namespace Mechanics.Application.Payments;

public class PaymentGatewayResult
{
    public string? PaymentId { get; init; }
    public string? PreferenceId { get; init; }
    public string? InitPoint { get; init; }
    public string? SandboxInitPoint { get; init; }
    public string? Status { get; init; }
    public string? StatusDetail { get; init; }
    public required string ExternalReference { get; init; }
}
