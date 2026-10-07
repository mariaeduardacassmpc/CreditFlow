using Application.Dtos.Auth;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Services;

public class AuthService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
{
    public async Task<User> Register(RegisterDto dto)
    {
        var existingUser = await userRepository.GetByEmailAsync(dto.Email);

        if (existingUser is not null)
            throw new InvalidOperationException("E-mail já cadastrado.");

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = string.Empty,
            Active = true
        };

        user.PasswordHash = passwordHasher.HashPassword(
            user,
            dto.Password);

        return await userRepository.CreateAsync(user);
    }

    public async Task<LoginResponseDto> Login(LoginDto dto)
    {
        var user = await userRepository.GetByEmailAsync(dto.Email);

        if (user is null || !user.Active)
            throw new UnauthorizedAccessException("E-mail ou senha inválidos.");

        var result = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.Password);

        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("E-mail ou senha inválidos.");

        var token = "JWT_AQUI";

        return new LoginResponseDto
        {
            Token = token,
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email
        };
    }
}
