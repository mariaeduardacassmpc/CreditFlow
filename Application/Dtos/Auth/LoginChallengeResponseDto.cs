namespace Application.Dtos.Auth;

public class LoginChallengeResponseDto
{
    public required string ChallengeId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public required string Message { get; set; }
}