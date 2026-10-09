namespace Application.Dtos.Auth;

public class VerifyTwoFactorDto
{
    public required string ChallengeId { get; set; }
    public required string Code { get; set; }
}