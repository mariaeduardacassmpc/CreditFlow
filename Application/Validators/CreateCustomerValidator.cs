using Application.Dtos.Customers;
using FluentValidation;

namespace Application.Validators;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(150);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(254);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .Matches(@"^(?:\+?55\s?)?\(?[1-9]\d\)?\s?9?\d{4}-?\d{4}$")
            .WithMessage("Informe um telefone brasileiro válido com DDD.");

        RuleFor(x => x.BirthDate)
            .NotEmpty()
            .Must(date => date.HasValue && date.Value.Date <= DateTime.Now.Date)
            .WithMessage("A data de nascimento é obrigatória e não pode ser futura.");
    }
}