using Application.Dtos.Auth;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Application.Services;

public class AuthService(CreditFlowDbContext context, IPasswordHasher<User> passwordHasher, ILogger<AuthService> logger, ITokenService tokenService, IMemoryCache memoryCache, IEmailService emailService)
{
    public async Task Register(RegisterDto dto)
    {
        logger.LogInformation("Criando úsuario.");

        var existingUser = await context.Users
            .FirstOrDefaultAsync(x => x.Email == dto.Email);

        if (existingUser is not null)
            throw new InvalidOperationException("E-mail já cadastrado.");

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = string.Empty,
            Active = true
        };

        PasswordValidator.Validate(dto.Password);

        user.PasswordHash = passwordHasher.HashPassword(user, dto.Password);

        context.Users.Add(user);

        await context.SaveChangesAsync();

        logger.LogInformation("Úsuario criado com sucesso.");
    }

    public async Task<AuthResponseDto> Login(LoginDto dto)
    {
        logger.LogInformation("Tentativa de login para o e-mail: {Email}", dto.Email);

        var user = await context.Users
            .FirstOrDefaultAsync(x => x.Email == dto.Email);

        if (user is null || !user.Active)
            throw new UnauthorizedAccessException("E-mail ou senha inválidos.");

        var result = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.Password);

        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("E-mail ou senha inválidos.");

        var (token, expiresAt) = tokenService.GenerateToken(user);

        logger.LogInformation("Login realizado com sucesso para o usuário {UserId}.", user.UserId);
        
        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserResponseDto
            {
                Id = user.UserId,
                Email = user.Email
            }
        };
    }

    public async Task ForgotPassword(ForgotPasswordDto dto)
    {
        logger.LogInformation("Solicitação de redefinição de senha para o e-mail: {Email}", dto.Email);

        var user = await context.Users
            .SingleOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
        {
            logger.LogWarning("Redefinição de senha falhou. Usuário não encontrado para o e-mail: {Email}", dto.Email);
            throw new InvalidOperationException($"Usuário não encontrado.");
        }

        var resetCode = RandomNumberGenerator
            .GetInt32(100000, 1000000)
            .ToString();

        memoryCache.Set(
            $"password-reset:{resetCode}",
            user.Email,
            TimeSpan.FromMinutes(30)
        );

        await emailService.SendPasswordResetEmail(
            user.Email,
            resetCode
        );

        logger.LogInformation("E-mail de redefinição enviado com sucesso.");
    }

    public async Task ResetPassword(PasswordResetDto dto)
    {
        var cacheKey = $"password-reset:{dto.Code}";

        if (!memoryCache.TryGetValue(cacheKey, out string? email))
        {
            throw new InvalidOperationException(
                "Código de redefinição de senha inválido ou expirado."
            );
        }

        var user = await context.Users
            .SingleOrDefaultAsync(u => u.Email == email);

        if (user is null)
        {
            throw new InvalidOperationException(
                "Usuário não encontrado."
            );
        }

        PasswordValidator.Validate(dto.NewPassword);

        user.PasswordHash = passwordHasher.HashPassword(
            user,
            dto.NewPassword
        );

        await context.SaveChangesAsync();

        memoryCache.Remove(cacheKey);
    }
}