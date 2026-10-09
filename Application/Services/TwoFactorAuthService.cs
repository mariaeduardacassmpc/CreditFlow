using Application.Dtos.Auth;
using Application.Interfaces;
using Domain.Entities;
using Domain.Events;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services;

public class TwoFactorAuthService(
    IUserRepository userRepository,
    ITokenService tokenService,
    IKafkaProducer kafkaProducer,
    IMemoryCache cache,
    IConfiguration configuration,
    ILogger<TwoFactorAuthService> logger
) : ITwoFactorAuthService
{
    private string GetSecret()
    {
        var secret = configuration["TwoFactor:Secret"];
        if (string.IsNullOrEmpty(secret))
            throw new InvalidOperationException("TwoFactor secret not configured.");
        return secret;
    }

    private static string ComputeHmacHex(string challengeId, string code, byte[] key)
    {
        using var hmac = new HMACSHA256(key);
        var data = Encoding.UTF8.GetBytes($"{challengeId}:{code}");
        var hash = hmac.ComputeHash(data);
        return Convert.ToHexString(hash);
    }

    public async Task<LoginChallengeResponseDto> CreateChallenge(
        User user)
    {
        var code = RandomNumberGenerator
            .GetInt32(100000, 1000000)
            .ToString();

        var challengeId = Convert.ToHexString(
            RandomNumberGenerator.GetBytes(32));

        var cacheKey = $"two-factor:{challengeId}";

        var secretKey = Encoding.UTF8.GetBytes(GetSecret());

        cache.Set(
            cacheKey,
            new TwoFactorChallenge
            {
                Email = user.Email,
                CodeHash = ComputeHmacHex(challengeId, code, secretKey),
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao publicar evento de 2FA para {Email}", user.Email);
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

        var secretKey = Encoding.UTF8.GetBytes(GetSecret());
        var suppliedHash = ComputeHmacHex(dto.ChallengeId, dto.Code, secretKey);

        var expectedBytes = Convert.FromHexString(challenge.CodeHash);
        var suppliedBytes = Convert.FromHexString(suppliedHash);

        if (!CryptographicOperations.FixedTimeEquals(expectedBytes, suppliedBytes))
        {
            challenge.Attempts++;
            logger.LogWarning("Falha 2FA para {Email}. Tentativas: {Attempts}", challenge.Email, challenge.Attempts);

            if (challenge.Attempts >= 5)
            {
                cache.Remove(cacheKey);
                logger.LogWarning("Bloqueio temporário aplicado para {Email}", challenge.Email);
            }
            else
            {
                cache.Set(cacheKey, challenge, TimeSpan.FromMinutes(5));
            }

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