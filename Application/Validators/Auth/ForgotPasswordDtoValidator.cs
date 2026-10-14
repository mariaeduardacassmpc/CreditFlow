using Application.Dtos.Password;
using FluentValidation;

namespace Application.Validators.Password;

public class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
{
    public ForgotPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("E-mail inválido.");
    }
}