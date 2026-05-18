using FluentValidation;
using Mechanics.Application.Budgets.Requests;

namespace Mechanics.Application.Budgets.Validators;

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
