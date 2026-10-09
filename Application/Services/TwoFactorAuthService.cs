using Application.Dtos.Auth;
using Application.Interfaces;
using Domain.Entities;
using Domain.Events;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services;

public class TwoFactorAuthService(
    IUserRepository userRepository,
    ITokenService tokenService,
    IKafkaProducer kafkaProducer,
    IMemoryCache cache
) : ITwoFactorAuthService
{
    public async Task<LoginChallengeResponseDto> CreateChallenge(
        User user)
    {
        var code = RandomNumberGenerator
            .GetInt32(100000, 1000000)
            .ToString();

        var challengeId = Convert.ToHexString(
            RandomNumberGenerator.GetBytes(32));

        var cacheKey = $"two-factor:{challengeId}";

        cache.Set(
            cacheKey,
            new TwoFactorChallenge
            {
                Email = user.Email,
                CodeHash = Convert.ToHexString(
                    SHA256.HashData(
                        Encoding.UTF8.GetBytes($"{challengeId}:{code}"))),
                Attempts = 0
            },
            TimeSpan.FromMinutes(5));

        try
        {
            await kafkaProducer.PublishAsync(
                "two-factor-code-requested",
                new TwoFactorCodeRequestedEvent
                {
                    Email = user.Email,
                    Code = code
                });
        }
        catch
        {
            cache.Remove(cacheKey);
            throw;
        }

        return new LoginChallengeResponseDto
        {
            ChallengeId = challengeId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            Message = "Código de autenticação enviado para seu e-mail."
        };
    }

    public async Task<AuthResponseDto> VerifyCode(VerifyTwoFactorDto dto)
    {
        var cacheKey = $"two-factor:{dto.ChallengeId}";

        if (!cache.TryGetValue(
                cacheKey, out TwoFactorChallenge? challenge) ||
            challenge is null)
        {
            throw new InvalidOperationException("Código inválido ou expirado.");
        }

        if (dto.Code.Length != 6 ||
            !dto.Code.All(char.IsDigit))
        {
            throw new InvalidOperationException("Código inválido ou expirado.");
        }

        var suppliedHash = Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(
                    $"{dto.ChallengeId}:{dto.Code}")));

        var expectedBytes = Convert.FromHexString(challenge.CodeHash);
        var suppliedBytes = Convert.FromHexString(suppliedHash);

        if (!CryptographicOperations.FixedTimeEquals(expectedBytes, suppliedBytes))
        {
            challenge.Attempts++;

            if (challenge.Attempts >= 5)
                cache.Remove(cacheKey);

            throw new InvalidOperationException("Código inválido ou expirado.");
        }

        cache.Remove(cacheKey);

        var user = await userRepository.GetByEmailAsync(challenge.Email);

        if (user is null || !user.Active)
        {
            throw new InvalidOperationException("Usuário inválido ou inativo.");
        }

        var (token, expiresAt) = tokenService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt
        };
    }
}

public class TwoFactorChallenge
{
    public required string Email { get; set; }
    public required string CodeHash { get; set; }
    public int Attempts { get; set; }
}