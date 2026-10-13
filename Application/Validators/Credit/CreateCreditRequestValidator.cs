using Application.Dtos.Credit;
using FluentValidation;

namespace Application.Validators.Credit;

public class CreateCreditRequestDtoValidator
    : AbstractValidator<CreateCreditRequestDto>
{
    public CreateCreditRequestDtoValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .WithMessage("O cliente informado é inválido.");

        RuleFor(x => x.RequestedAmount)
            .GreaterThan(0)
            .WithMessage("O valor solicitado deve ser maior que zero.")
            .LessThanOrEqualTo(1_000_000m)
            .WithMessage("O valor solicitado excede o limite permitido.");

        RuleFor(x => x.MonthlyIncome)
            .GreaterThan(0)
            .WithMessage("A renda mensal deve ser maior que zero.");

        RuleFor(x => x.CreditScore)
            .InclusiveBetween(0, 1000)
            .WithMessage("O score de crédito deve estar entre 0 e 1000.");

        RuleFor(x => x.EmploymentMonths)
            .GreaterThanOrEqualTo(0)
            .WithMessage("O tempo de emprego não pode ser negativo.");

        RuleFor(x => x.TermMonths)
            .InclusiveBetween(1, 120)
            .WithMessage("O prazo deve estar entre 1 e 120 meses.");

        RuleFor(x => x.Purpose)
            .IsInEnum()
            .WithMessage("A finalidade do crédito é inválida.");
    }
}