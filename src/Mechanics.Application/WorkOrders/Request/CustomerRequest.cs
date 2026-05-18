using Mechanics.Application.WorkOrders.Responses;
using Mechanics.Domain.Customers;

namespace Mechanics.Application.WorkOrders.Request;

public class CustomerRequest
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required PersonalDocument Document { get; init; }
}
