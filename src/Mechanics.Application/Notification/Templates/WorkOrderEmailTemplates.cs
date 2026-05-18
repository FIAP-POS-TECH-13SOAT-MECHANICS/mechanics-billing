
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Integrations.EmailSender;
using System.Text;
using Mechanics.Application.WorkOrders.Request;


namespace Mechanics.Application.Notification.Templates;

public static class WorkOrderEmailTemplates
{

    public static EmailMessage WorkOrderPendingApproval(CustomerRequest customer, WorkOrderRequest workOrder, Budget budget) => new()
    {
        Recipient = customer.Email,
        Subject = "Orçamento da OS disponível - FIAP Mechanics",
        Body = BuildPendingApprovalBody(customer, workOrder, budget),
    };

    private static string BuildPendingApprovalBody(CustomerRequest customer, WorkOrderRequest workOrder, Budget budget)
    {
        var sb = new StringBuilder();

        sb.Append($"""
                   <p>Olá, <b>{customer.Name}</b>,</p>
                   <p>O orçamento da sua ordem de serviço está pronto e aguarda sua aprovação.</p>
                   <ul>
                       <li><b>Ordem</b>: {workOrder.AccessKey}</li>
                       <li><b>Orçamento</b>: {budget.Id}</li>
                       <li><b>Valor estimado</b>: {budget.Total:C}</li>
                   </ul>
                   """);

        sb.Append("<p>Resumo dos itens:</p><ul>");
        if (budget.Items != null && budget.Items.Count != 0)
            foreach (var item in budget.Items)
                sb.Append($"<li>{item.NameSnapshot} — {item.Quantity}x {item.UnitPriceSnapshot:C} = {item.Subtotal:C}</li>");
        else
            sb.Append("<li>— Nenhum item listado —</li>");
        sb.Append("</ul>");

        sb.Append("<p>Para aprovar ou rejeitar o orçamento, acesse nosso sistema.</p><br />");

        sb.Append($"<p>O orçamento expira em: {budget.ExpiresAt?.ToString("t") ?? "—"}</p>");
        sb.Append("<p>Obrigado,<br/>FIAP Mechanics</p>");

        return sb.ToString();
    }

}
