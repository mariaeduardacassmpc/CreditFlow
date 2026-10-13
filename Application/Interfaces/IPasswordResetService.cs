using Application.Dtos.Password;

namespace Application.Interfaces;

public interface IPasswordResetService
{
    Task ForgotPassword(ForgotPasswordDto dto, CancellationToken cancellation);
    Task ResetPassword(PasswordResetDto dto, CancellationToken cancellation);
}