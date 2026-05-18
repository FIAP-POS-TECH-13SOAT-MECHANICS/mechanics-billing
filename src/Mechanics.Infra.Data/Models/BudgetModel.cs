using Amazon.DynamoDBv2.DataModel;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Data.TypeConverters;

namespace Mechanics.Infra.Data.Models;

[DynamoDBTable(nameof(BudgetModel), LowerCamelCaseProperties = true)]
public class BudgetModel
{
    [DynamoDBHashKey(typeof(GuidTypeConverter))]
    public required Guid Id { get; init; }

    [DynamoDBGlobalSecondaryIndexHashKey]
    public required Guid WorkOrderId { get; init; }

    [DynamoDBGlobalSecondaryIndexHashKey]
    public required Guid CustomerId { get; init; }

    [DynamoDBProperty(typeof(GuidTypeConverter))]
    public required Guid VehicleId { get; init; }

    [DynamoDBProperty]
    public required decimal Total { get; init; }

    [DynamoDBProperty]
    public required DateTime CreationDate { get; init; }

    [DynamoDBProperty]
    public required DateTime ExpiresAt { get; init; }

    [DynamoDBProperty(typeof(EnumConverter<BudgetStatus>))]
    public required BudgetStatus Status { get; init; }

    [DynamoDBProperty]
    public required List<BudgetItemModel> Items { get; init; }

    public BudgetNew ToModel()
    {
        return new BudgetNew
        {
            Id = Id,
            CreationDate = CreationDate,
            WorkOrderId = WorkOrderId,
            CustomerId = CustomerId,
            VehicleId = VehicleId,
            Total = Total,
            ExpiresAt = ExpiresAt,
            Status = Status,
            Items = Items.Select(item => new BudgetNewItem
            {
                Id = item.Id,
                Name = item.Name,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                Subtotal = item.Subtotal,
            }).ToList(),
        };
    }

    public static BudgetModel FromModel(BudgetNew budget)
    {
        return new BudgetModel
        {
            Id = budget.Id,
            CreationDate = budget.CreationDate,
            WorkOrderId = budget.WorkOrderId,
            CustomerId = budget.CustomerId,
            VehicleId = budget.VehicleId,
            Total = budget.Total,
            ExpiresAt = budget.ExpiresAt.DateTime,
            Status = budget.Status,
            Items = budget.Items.Select(item => new BudgetItemModel
            {
                Id = item.Id,
                Name = item.Name,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                Subtotal = item.Subtotal,
            }).ToList(),
        };
    }
}

public class BudgetItemModel
{
    [DynamoDBProperty(typeof(GuidTypeConverter))]
    public required Guid Id { get; init; }

    [DynamoDBProperty]
    public required string Name { get; init; }

    [DynamoDBProperty]
    public required decimal UnitPrice { get; init; }

    [DynamoDBProperty]
    public required int Quantity { get; init; }

    [DynamoDBProperty]
    public required decimal Subtotal { get; init; }
}
