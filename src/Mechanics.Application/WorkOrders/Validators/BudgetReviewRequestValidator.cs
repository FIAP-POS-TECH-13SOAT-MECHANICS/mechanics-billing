using FluentValidation;
using Mechanics.Application.WorkOrders.Requests;

namespace Mechanics.Application.WorkOrders.Validators;

public class BudgetReviewRequestValidator : AbstractValidator<BudgetReviewRequest>
{
    public BudgetReviewRequestValidator()
    {
        RuleFor(request => request.AccessKey).NotEmpty().Length(8);
        RuleFor(request => request.Payment!.Installments)
            .GreaterThan(0)
            .When(request => request.Payment is not null);
    }
}
