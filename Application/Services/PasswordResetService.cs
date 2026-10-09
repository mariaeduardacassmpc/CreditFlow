using Application.Dtos.Password;
using Application.Interfaces;
using Domain.Entities;
using Domain.Events;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Application.Services;

public class PasswordResetService(
    IUserRepository repository,
    IPasswordHasher<User> passwordHasher,
    ILogger<PasswordResetService> logger,
    IMemoryCache memoryCache,
    IEmailService emailService,
    IKafkaProducer kafkaProducer
) : IPasswordResetService
{
    public async Task ForgotPassword(ForgotPasswordDto dto)
    {
        logger.LogInformation("Solicitação de redefinição de senha recebida.");

        var user = await repository.GetByEmailAsync(dto.Email);

        if (user is null)
        {
            logger.LogInformation("Solicitação de redefinição processada.");
            return;
        }

        var resetCode = RandomNumberGenerator
            .GetInt32(100000, 1000000)
            .ToString();

        var cacheKey = $"password-reset:{resetCode}";

        memoryCache.Set(
           cacheKey,
           user.Email,
           TimeSpan.FromMinutes(30));

        try
        {
            await kafkaProducer.PublishAsync(
                "password-reset-requested",
                new PasswordResetRequestedEvent
                {
                    Email = user.Email,
                    Code = resetCode
                });

            logger.LogInformation(
                "Evento de redefinição publicado no Kafka.");
        }
        catch
        {
            memoryCache.Remove(cacheKey);
            throw;
        }
    }

    public async Task ResetPassword(PasswordResetDto dto)
    {
        var cacheKey = $"password-reset:{dto.Code}";

        if (!memoryCache.TryGetValue(cacheKey, out string? email))
        {
            throw new InvalidOperationException("Código de redefinição de senha inválido ou expirado.");
        }

        var user = await repository.GetByEmailAsync(email!);

        if (user is null)
        {
            throw new InvalidOperationException("Usuário não encontrado.");
        }

        PasswordValidator.Validate(dto.NewPassword);

        user.PasswordHash = passwordHasher.HashPassword(user, dto.NewPassword);

        await repository.SaveChangesAsync();

        memoryCache.Remove(cacheKey);

        logger.LogInformation("Senha redefinida com sucesso para o usuário {UserId}.", user.UserId);
    }
}