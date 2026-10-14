using Application.Dtos.Auth;
using FluentValidation;

namespace Application.Validators.Auth;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(150);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(254);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A senha é obrigatória.")
            .MinimumLength(8).WithMessage("A senha deve ter ao menos 8 caracteres.")
            .Matches(@"[A-Z]").WithMessage("A senha deve conter pelo menos uma letra maiúscula.")
            .Matches(@"[a-z]").WithMessage("A senha deve conter pelo menos uma letra minúscula.")
            .Matches(@"\d").WithMessage("A senha deve conter pelo menos um número.")
            .Matches(@"[^\da-zA-Z]").WithMessage("A senha deve conter pelo menos um caractere especial.");
    }
}