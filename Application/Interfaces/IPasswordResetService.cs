using Application.Dtos.Password;

namespace Application.Interfaces;

public interface IPasswordResetService
{
    Task ForgotPassword(ForgotPasswordDto dto);
    Task ResetPassword(PasswordResetDto dto);
}