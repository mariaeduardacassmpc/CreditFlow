namespace Application.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmail(string email, string resetCode);
    Task SendTwoFactorCodeEmail(string email, string code);
}