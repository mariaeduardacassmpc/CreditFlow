namespace Application.Dtos.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserResponseDto User { get; set; } = null!;
}

public class UserResponseDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
}