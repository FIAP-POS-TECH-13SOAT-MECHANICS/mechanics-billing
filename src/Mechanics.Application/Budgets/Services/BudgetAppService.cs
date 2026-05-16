using Mechanics.Application.Notification.Services;
using Mechanics.Application.Observability;
using Mechanics.Application.Utils;
using Mechanics.Application.WorkOrders.Responses;
using Mechanics.Application.WorkOrders.Services;
using Mechanics.Domain.Customers;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Data.Models;
using Mechanics.Infra.Data.Repositories;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Mechanics.Application.Budgets.Services;

/// <summary>
///     Serviço para criação, envio e aprovação pública de budgets.
/// </summary>
public class BudgetAppService(
    IEmailService emailService,
    ILogger<BudgetAppService> logger,
    IWorkOrdersApiService workOrdersApiService,
    IBudgetRepository budgetRepository)
    : IAppService
{
    public async Task CreateAndSendBudget(Events.BudgetCreatedEvent budgetCreatedEvent,
        CancellationToken cancellationToken = default)
    {
        var budget = new BudgetNew
        {
            Id = budgetCreatedEvent.EventId,
            CreationDate = budgetCreatedEvent.OccurredAt.DateTime,
            WorkOrderId = budgetCreatedEvent.WorkOrderId,
            CustomerId = budgetCreatedEvent.CustomerId,
            VehicleId = budgetCreatedEvent.VehicleId,
            Total = budgetCreatedEvent.Total,
            ExpiresAt = budgetCreatedEvent.ExpiresAt,
            Status = BudgetStatus.Sent,
            Items = budgetCreatedEvent.Items.Select(item => new BudgetNewItem
            {
                Id = item.Id,
                Name = item.Name,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                Subtotal = item.Subtotal,
            }).ToList(),
        };

        await budgetRepository.Upsert(BudgetModel.FromModel(budget), cancellationToken);

        var workOrderResponse = await workOrdersApiService.GetWorkOrderById(budgetCreatedEvent.WorkOrderId, cancellationToken);
        var customerResponse = await workOrdersApiService.GetCustomerById(budgetCreatedEvent.CustomerId, cancellationToken);

        if (workOrderResponse is null || customerResponse is null)
        {
            logger.LogWarning(
                "Budget {BudgetId} persisted, but approval email was skipped. WorkOrderFound={WorkOrderFound}, CustomerFound={CustomerFound}",
                budget.Id,
                workOrderResponse is not null,
                customerResponse is not null);
            return;
        }

        try
        {
            await emailService.SendWorkOrderPendingApproval(
                ToCustomer(customerResponse),
                ToWorkOrder(workOrderResponse),
                ToLegacyBudget(budget),
                cancellationToken);

            AppMetrics.EmailsSent.Add(1, new TagList
            {
                { "template", "budget_pending_approval" },
            });
        }
        catch (Exception ex)
        {
            AppMetrics.EmailsFailed.Add(1, new TagList
            {
                { "template", "budget_pending_approval" },
            });
            logger.LogWarning(ex, "Failed to send pending approval email for WorkOrder {WorkOrderId}", budget.WorkOrderId);
        }
    }

#if false
    /// <summary>
    ///     Cria um <see cref="Budget"/> a partir dos produtos/serviços atualmente associados à WorkOrder,
    ///     persiste snapshot de preços e itens, define ExpiresAt = CreationDate + 3 dias,
    ///     atualiza <see cref="WorkOrder.Status"/> para PendingApproval e registra o usuário responsável.
    /// </summary>
    public async Task CreateAndSendBudget(Guid workOrderId, Guid performedByUserId, CancellationToken cancellationToken = default)
    {
        if (performedByUserId == Guid.Empty)
            throw new BusinessException("PerformedByUserId must be informed.");

        var workOrderResponse = await workOrdersApiService.GetWorkOrderById(workOrderId, cancellationToken);
        EntityNotFoundException.ThrowIfNotFound<WorkOrder>(workOrderResponse is not null, workOrderId);

        var wo = ToWorkOrder(workOrderResponse);
        var workOrderProducts = workOrderResponse.Products?.ToList() ?? [];
        var serviceCatalogIds = workOrderResponse.ServiceCatalogIds?.ToList() ?? [];

        if (workOrderProducts.Count == 0 && serviceCatalogIds.Count == 0)
            throw new BusinessException("Order must contain at least one product or service to create a budget.");

        var now = DateTime.Now;
        var budget = new Budget
        {
            WorkOrderId = wo.Id,
            CreationDate = now,
            ExpiresAt = now.AddDays(3),
            Status = BudgetStatus.Sent,
            Items = new List<BudgetItem>(),
        };

        var partsTotal = 0m;
        if (workOrderProducts.Count > 0)
        {
            var productIds = workOrderProducts.Select(p => p.ProductId).ToList();
            var products = await dbContext.Products.Where(p => productIds.Contains(p.Id)).ToListAsync(cancellationToken);

            foreach (var workOrderProduct in workOrderProducts)
            {
                var product = products.First(product => product.Id == workOrderProduct.ProductId);
                var item = new BudgetItem
                {
                    BudgetId = budget.Id,
                    ProductId = product.Id,
                    NameSnapshot = product.Name,
                    UnitPriceSnapshot = product.UnitPrice,
                    Quantity = workOrderProduct.Quantity,
                    Subtotal = product.UnitPrice * workOrderProduct.Quantity,
                };
                partsTotal += item.Subtotal;
                budget.Items.Add(item);
            }
        }

        var servicesTotal = 0m;
        if (serviceCatalogIds.Count > 0)
        {
            var services = await dbContext.ServiceCatalog.Where(s => serviceCatalogIds.Contains(s.Id))
                .ToListAsync(cancellationToken);

            foreach (var item in services.Select(s => new BudgetItem
                     {
                         BudgetId = budget.Id,
                         ServiceCatalogId = s.Id,
                         NameSnapshot = s.Name,
                         UnitPriceSnapshot = s.BasePrice,
                         Quantity = 1, // serviços são individuais
                         Subtotal = s.BasePrice,
                     }))
            {
                servicesTotal += item.Subtotal;
                budget.Items.Add(item);
            }
        }

        budget.Total = partsTotal + servicesTotal;

        await dbContext.Budgets.AddAsync(budget, cancellationToken);

        var previousStatus = wo.Status;
        var timeInPreviousStatus = DateTime.Now - wo.LastUpdate;

        wo.ApprovalRequestedAt = now;
        wo.Status = WorkOrderStatus.PendingApproval;
        wo.LastStatusChangeBy ??= performedByUserId;
        wo.LastUpdate = now;

        var tags = new TagList
        {
            { "previous_status", previousStatus.ToString() },
            { "new_status", nameof(WorkOrderStatus.PendingApproval) },
        };

        AppMetrics.TimeInStatusTotalSeconds.Add(
            Math.Round(timeInPreviousStatus.TotalSeconds, 2), tags);
        AppMetrics.TimeInStatusSamples.Add(1, tags);
        AppMetrics.StatusTransitions.Add(1, tags);

        AppMetrics.StatusDurationSeconds.Record(
            Math.Round(timeInPreviousStatus.TotalSeconds, 2),
            new TagList { { "status", previousStatus.ToString() } });

        await dbContext.SaveChangesAsync(cancellationToken);

        var hist = new WorkOrderHistory
        {
            WorkOrderId = wo.Id,
            Action = "BudgetSent",
            Details = $"Budget {budget.Id} sent. Total: {budget.Total:C}",
            PerformedByUserId = performedByUserId,
        };
        await dbContext.WorkOrderHistories.AddAsync(hist, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        var customerResponse = await workOrdersApiService.GetCustomerById(wo.CustomerId, cancellationToken);
        if (customerResponse == null)
            return;

        var customer = ToCustomer(customerResponse);

        try
        {
            await emailService.SendWorkOrderPendingApproval(customer, wo, budget, cancellationToken);
            AppMetrics.EmailsSent.Add(1, new TagList
            {
                { "template", "budget_pending_approval" }
            });
        }
        catch (Exception ex)
        {
            AppMetrics.EmailsFailed.Add(1, new TagList
            {
                { "template", "budget_pending_approval" }
            });
            logger.LogWarning(ex, "Failed to send pending approval email for WorkOrder {WorkOrderId}", wo.Id);
        }
    }

    /// <summary>
    ///     Aprova um orçamento.
    /// </summary>
    public async Task<BudgetReviewResponse> ApproveBudget(Guid customerId, string accessKey, string? description = null,
        BudgetPaymentRequest? paymentRequest = null, CancellationToken cancellationToken = default)
    {
        var normalizedAccessKey = accessKey.Replace(" ", "");

        var customer = await dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);
        EntityNotFoundException.ThrowIfNull(customer, customerId);

        var wo = await dbContext.WorkOrders
            .FirstOrDefaultAsync(w => w.CustomerId == customer.Id && w.AccessKey == normalizedAccessKey, cancellationToken);
        EntityNotFoundException.ThrowIfNull(wo, accessKey);

        var budget = await dbContext.Budgets
            .Where(b => b.WorkOrderId == wo.Id && b.Status == BudgetStatus.Sent)
            .OrderByDescending(b => b.CreationDate)
            .Include(b => b.Items)
            .FirstOrDefaultAsync(cancellationToken);
        EntityNotFoundException.ThrowIfNull(budget, budget?.WorkOrderId);

        if (budget.ApprovedAt != null)
            throw new BusinessException("Budget already approved.");

        if (budget.ExpiresAt.HasValue && DateTime.Now > budget.ExpiresAt.Value)
        {
            budget.Status = BudgetStatus.Expired;
            await dbContext.SaveChangesAsync(cancellationToken);
            throw new BusinessException("Budget expired.");
        }

        PaymentGatewayResult? payment = null;
        if (paymentRequest is not null)
        {
            payment = await paymentGateway.CreatePaymentAsync(new CreatePaymentInput
            {
                WorkOrderId = wo.Id,
                BudgetId = budget.Id,
                Amount = budget.Total,
                Description = $"Work order {wo.AccessKey}",
                PaymentMethodId = paymentRequest.PaymentMethodId ?? "checkout_pro",
                Token = paymentRequest.Token,
                Installments = paymentRequest.Installments,
                IssuerId = paymentRequest.IssuerId,
                PayerEmail = paymentRequest.PayerEmail ?? customer.Email,
            }, cancellationToken);
        }

        var previousStatus = wo.Status;
        var timeInPreviousStatus = DateTime.Now - wo.LastUpdate;

        // Approve
        budget.Status = BudgetStatus.Approved;
        budget.ApprovedAt = DateTime.Now;
        budget.ApprovedByCustomerDocument = customer.Document.Number;
        budget.Description = description;

        wo.Status = WorkOrderStatus.InProgress;
        wo.LastUpdate = DateTime.Now;


        var tags = new TagList
        {
            { "previous_status", previousStatus.ToString() },
            { "new_status", nameof(WorkOrderStatus.InProgress) }
        };

        AppMetrics.TimeInStatusTotalSeconds.Add(
            Math.Round(timeInPreviousStatus.TotalSeconds, 2), tags);
        AppMetrics.TimeInStatusSamples.Add(1, tags);
        AppMetrics.StatusTransitions.Add(1, tags);

        AppMetrics.StatusDurationSeconds.Record(
            Math.Round(timeInPreviousStatus.TotalSeconds, 2),
            new TagList { { "status", previousStatus.ToString() } });

        var hist = new WorkOrderHistory
        {
            WorkOrderId = wo.Id,
            Action = "BudgetApprovedPublic",
            Details = description is null
                ? $"Budget {budget.Id} approved by customer {customer.Document.Number}."
                : $"Budget {budget.Id} approved by customer {customer.Document.Number}. Description: {description}",
            PerformedByUserId = null,
        };
        await dbContext.WorkOrderHistories.AddAsync(hist, cancellationToken);

        if (payment is not null)
            await dbContext.WorkOrderHistories.AddAsync(new WorkOrderHistory
            {
                WorkOrderId = wo.Id,
                Action = "PaymentCreated",
                Details = $"Mercado Pago preference {payment.PreferenceId} created. Amount: {budget.Total:C}",
                PerformedByUserId = null,
            }, cancellationToken);

        if (payment is not null)
            await paymentService.RegisterCreatedPreferenceAsync(wo.Id, budget.Id, budget.Total, payment, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        if (payment is not null)
        {
            try
            {
                await eventPublisher.PublishAsync(new PaymentCreatedEvent
                {
                    WorkOrderId = wo.Id,
                    BudgetId = budget.Id,
                    Amount = budget.Total,
                    PaymentId = payment.PaymentId,
                    PreferenceId = payment.PreferenceId,
                    InitPoint = payment.InitPoint,
                    SandboxInitPoint = payment.SandboxInitPoint,
                    Status = payment.Status,
                    StatusDetail = payment.StatusDetail,
                    ExternalReference = payment.ExternalReference,
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to publish payment created event for WorkOrder {WorkOrderId}", wo.Id);
            }
        }

        try
        {
            await emailService.SendWorkOrderStatusChanged(customer, wo, WorkOrderStatus.PendingApproval, cancellationToken);
            AppMetrics.EmailsSent.Add(1, new TagList
            {
                { "template", "budget_approved_customer" }
            });
        }
        catch (Exception ex)
        {
            AppMetrics.EmailsFailed.Add(1, new TagList
            {
                { "template", "budget_approved_customer" }
            });
            logger.LogWarning(ex, "Failed to send status changed email after budget approval for WorkOrder {WorkOrderId}",
                wo.Id);
        }

        if (wo.AssignedToUserId != null)
        {
            var mechanic = await dbContext.Users.FindAsync([wo.AssignedToUserId], cancellationToken);
            EntityNotFoundException.ThrowIfNull(mechanic, wo.AssignedToUserId);

            try
            {
                await emailService.SendMechanicBudgetDecision(mechanic, wo, budget, approved: true, cancellationToken);
                AppMetrics.EmailsSent.Add(1, new TagList
                {
                    { "template", "budget_approved_customer" }
                });
            }
            catch (Exception ex)
            {
                AppMetrics.EmailsFailed.Add(1, new TagList
                {
                    { "template", "budget_approved_customer" }
                });
                logger.LogWarning(ex, "Failed to send mechanic notification for approved budget {BudgetId}", budget.Id);
            }
        }

        return new BudgetReviewResponse
        {
            WorkOrderId = wo.Id,
            BudgetId = budget.Id,
            Payment = payment is null
                ? null
                : new PaymentReviewResponse
                {
                    PaymentId = payment.PaymentId,
                    PreferenceId = payment.PreferenceId,
                    InitPoint = payment.InitPoint,
                    SandboxInitPoint = payment.SandboxInitPoint,
                    Status = payment.Status,
                    StatusDetail = payment.StatusDetail,
                    ExternalReference = payment.ExternalReference,
                },
        };
    }

    /// <summary>
    ///     Rejeita um orçamento.
    ///     Marca o budget como Rejected, coloca a OS novamente em UnderDiagnosis e notifica o mecânico.
    /// </summary>
    public async Task RejectBudget(Guid customerId, string accessKey, string? description = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedAccessKey = accessKey.Replace(" ", "");

        var customer = await dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);
        EntityNotFoundException.ThrowIfNull(customer, customerId);

        var wo = await dbContext.WorkOrders
            .FirstOrDefaultAsync(w => w.CustomerId == customer.Id && w.AccessKey == normalizedAccessKey, cancellationToken);
        EntityNotFoundException.ThrowIfNull(wo, accessKey);

        var budget = await dbContext.Budgets
            .Where(b => b.WorkOrderId == wo.Id && b.Status == BudgetStatus.Sent)
            .OrderByDescending(b => b.CreationDate)
            .Include(b => b.Items)
            .FirstOrDefaultAsync(cancellationToken);
        EntityNotFoundException.ThrowIfNull(budget, wo.Id);

        if (budget.ExpiresAt.HasValue && DateTime.Now > budget.ExpiresAt.Value)
        {
            budget.Status = BudgetStatus.Expired;
            await dbContext.SaveChangesAsync(cancellationToken);
            throw new BusinessException("Budget expired.");
        }

        var previousStatus = wo.Status;
        var timeInPreviousStatus = DateTime.Now - wo.LastUpdate;

        budget.Status = BudgetStatus.Rejected;
        budget.RejectedAt = DateTime.Now;
        budget.Description = description;

        wo.Status = WorkOrderStatus.UnderDiagnosis;
        wo.LastUpdate = DateTime.Now;

        var tags = new TagList
        {
            { "previous_status", previousStatus.ToString() },
            { "new_status", nameof(WorkOrderStatus.UnderDiagnosis) }
        };

        AppMetrics.TimeInStatusTotalSeconds.Add(
            Math.Round(timeInPreviousStatus.TotalSeconds, 2), tags);
        AppMetrics.TimeInStatusSamples.Add(1, tags);
        AppMetrics.StatusTransitions.Add(1, tags);

        AppMetrics.StatusDurationSeconds.Record(
            Math.Round(timeInPreviousStatus.TotalSeconds, 2),
            new TagList { { "status", previousStatus.ToString() } });

        var hist = new WorkOrderHistory
        {
            WorkOrderId = wo.Id,
            Action = "BudgetRejectedByCustomer",
            Details = description is null
                ? $"Budget {budget.Id} rejected by customer {customer.Document.Number}."
                : $"Budget {budget.Id} rejected by customer {customer.Document.Number}. Description: {description}",
            PerformedByUserId = null,
        };
        await dbContext.WorkOrderHistories.AddAsync(hist, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        var customerEntity = await dbContext.Customers.FindAsync([wo.CustomerId], cancellationToken);
        if (customerEntity != null)
        {
            try
            {
                await emailService.SendWorkOrderStatusChanged(customerEntity, wo, WorkOrderStatus.PendingApproval,
                    cancellationToken);
                AppMetrics.EmailsSent.Add(1, new TagList
                {
                    { "template", "budget_rejected_customer" }
                });
            }
            catch (Exception ex)
            {
                AppMetrics.EmailsFailed.Add(1, new TagList
                {
                    { "template", "budget_rejected_customer" }
                });
                logger.LogWarning(ex, "Failed to send status changed email after budget rejection for WorkOrder {WorkOrderId}",
                    wo.Id);
            }
        }

        if (wo.AssignedToUserId != null)
        {
            var mechanic = await dbContext.Users.FindAsync([wo.AssignedToUserId], cancellationToken);
            EntityNotFoundException.ThrowIfNull(mechanic, wo.AssignedToUserId);

            try
            {
                await emailService.SendMechanicBudgetDecision(mechanic, wo, budget, approved: false, cancellationToken);
                AppMetrics.EmailsSent.Add(1, new TagList
                {
                    { "template", "budget_rejected_mechanic" }
                });
            }
            catch (Exception ex)
            {
                AppMetrics.EmailsFailed.Add(1, new TagList
                {
                    { "template", "budget_rejected_mechanic" }
                });
                logger.LogWarning(ex, "Failed to send mechanic notification for rejected budget {BudgetId}", budget.Id);
            }
        }
    }
#endif

    private static WorkOrder ToWorkOrder(WorkOrderResponse response) => new()
    {
        Id = response.Id,
        CreationDate = response.CreationDate,
        CustomerId = response.CustomerId,
        VehicleId = response.VehicleId,
        AccessKey = response.AccessKey,
        Status = response.Status,
        LastUpdate = response.LastUpdate ?? response.CreationDate,
        ReportedProblem = response.ReportedProblem,
        Observations = response.Observations,
    };

    private static Customer ToCustomer(CustomerResponse response) => new()
    {
        Id = response.Id,
        Name = response.Name,
        Email = response.Email,
        Document = new PersonalDocument(response.Document.Type, response.Document.Number),
    };

    private static Budget ToLegacyBudget(BudgetNew budget) => new()
    {
        Id = budget.Id,
        CreationDate = budget.CreationDate,
        WorkOrderId = budget.WorkOrderId,
        ExpiresAt = budget.ExpiresAt.DateTime,
        Status = budget.Status,
        Total = budget.Total,
        Items = budget.Items.Select(item => new BudgetItem
        {
            Id = item.Id,
            BudgetId = budget.Id,
            NameSnapshot = item.Name,
            UnitPriceSnapshot = item.UnitPrice,
            Quantity = item.Quantity,
            Subtotal = item.Subtotal,
        }).ToList(),
    };
}
