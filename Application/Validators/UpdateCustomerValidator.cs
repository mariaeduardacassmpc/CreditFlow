using Application.Dtos.Customers;
using FluentValidation;

namespace Application.Validators;

public class UpdateCustomerValidator : AbstractValidator<UpdateCustomerDto>
{
    public UpdateCustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().MinimumLength(2).MaximumLength(150);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .Matches(@"^(?:\+?55\s?)?\(?[1-9]\d\)?\s?9?\d{4}-?\d{4}$")
            .WithMessage("Informe um telefone brasileiro válido com DDD.");

        RuleFor(x => x.Email)
            .NotEmpty().EmailAddress().MaximumLength(254);

        RuleFor(x => x.BirthDate)
            .NotNull()
            .Must(date => date.HasValue && date.Value.Date <= DateTime.Now.Date)
            .WithMessage("A data de nascimento é obrigatória e não pode ser futura.");

        RuleFor(x => x.Cpf)
            .NotEmpty()
            .Matches(@"^\d{11}$")
            .WithMessage("O CPF deve conter exatamente 11 dígitos.");
    }
}