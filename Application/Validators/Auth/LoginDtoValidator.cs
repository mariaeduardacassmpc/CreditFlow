using Application.Dtos.Auth;
using FluentValidation;

namespace Application.Validators.Auth;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("E-mail inválido.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("A senha é obrigatória.");
    }
}