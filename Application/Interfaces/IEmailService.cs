namespace Application.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmail(string email, string token);
}