using Application.Dtos.Password;
using FluentValidation;

namespace Application.Validators.Password;

public class PasswordResetDtoValidator : AbstractValidator<PasswordResetDto>
{
    public PasswordResetDtoValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .Matches(@"^\d{6}$")
            .WithMessage("Código de redefinição inválido.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("A senha é obrigatória.")
            .MinimumLength(8).WithMessage("A senha deve ter ao menos 8 caracteres.")
            .Matches(@"[A-Z]").WithMessage("A senha deve conter pelo menos uma letra maiúscula.")
            .Matches(@"[a-z]").WithMessage("A senha deve conter pelo menos uma letra minúscula.")
            .Matches(@"\d").WithMessage("A senha deve conter pelo menos um número.")
            .Matches(@"[^\da-zA-Z]").WithMessage("A senha deve conter pelo menos um caractere especial.");
    }
}