using Application.Dtos.Auth;
using Domain.Entities;

namespace Application.Interfaces;

public interface ITwoFactorAuthService
{
    Task<LoginChallengeResponseDto> CreateChallenge(User user, CancellationToken cancellationToken);
    Task<AuthResponseDto> VerifyCode(VerifyTwoFactorDto dto, CancellationToken cancellationToken);
}