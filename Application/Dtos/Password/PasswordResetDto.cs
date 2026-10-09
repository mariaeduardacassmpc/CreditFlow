namespace Application.Dtos.Password;

public class PasswordResetDto
{
    public string Code { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}