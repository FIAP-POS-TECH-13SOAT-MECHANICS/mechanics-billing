using Mechanics.Application.Notification;
using Mechanics.Infra.Integrations.EmailSender;

namespace Mechanics.Application.Notification.Templates;

public static class PaymentEmailTemplates
{
    public static EmailMessage PaymentApproved(PaymentApprovedNotification notification) => new()
    {
        Recipient = notification.CustomerEmail,
        Subject = "Pagamento Aprovado - FIAP Mechanics",
        Body = $"""
                <p>Ola, <b>{notification.CustomerName}</b>,</p>
                <p>O seu pagamento foi aprovado.</p>
                <p>Valor aprovado: <b>R$ {notification.Amount:N2}</b>.</p>

                <p>Muito obrigado pela preferencia!</p>
                """,
    };
}
