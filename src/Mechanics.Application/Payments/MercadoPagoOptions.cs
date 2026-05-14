namespace Mechanics.Application.Payments;

public class MercadoPagoOptions
{
    public bool Enabled { get; init; } = true;
    public string AccessToken { get; init; } = string.Empty;
    public string? NotificationUrl { get; init; }
    public string? StatementDescriptor { get; init; }
}
