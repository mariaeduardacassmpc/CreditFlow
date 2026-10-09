using Application.Dtos.Auth;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
namespace Application.Services;

public class AuthService(
    IUserRepository repository, 
    IPasswordHasher<User> passwordHasher, 
    ILogger<AuthService> logger,
    ITwoFactorAuthService twoFactorAuthService
) : IAuthService
{
    public async Task Register(RegisterDto dto)
    {
        logger.LogInformation("Criando usuário.");

        var existingUser = await repository.GetByEmailAsync(dto.Email);

        if (existingUser is not null)
            throw new InvalidOperationException("E-mail já cadastrado.");

        PasswordValidator.Validate(dto.Password);

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = string.Empty,
            Active = true
        };

        user.PasswordHash = passwordHasher.HashPassword(user, dto.Password);

        await repository.AddAsync(user);
        await repository.SaveChangesAsync();

        logger.LogInformation("Usuário criado com sucesso.");
    }

    public async Task<LoginChallengeResponseDto> Login(LoginDto dto)
    {
        logger.LogInformation("Tentativa de login para o e-mail: {Email}", dto.Email);

        var user = await repository.GetByEmailAsync(dto.Email);

        if (user is null || !user.Active)
            throw new UnauthorizedAccessException("E-mail ou senha inválidos.");

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("E-mail ou senha inválidos.");

        return await twoFactorAuthService.CreateChallenge(user);
    }
}