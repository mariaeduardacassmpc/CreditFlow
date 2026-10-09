
using Application.Dtos.Auth;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace CreditFlow.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly Mock<IUserRepository> _repository = new();
    private readonly Mock<IPasswordHasher<User>> _passwordHasher = new();
    private readonly Mock<ILogger<AuthService>> _logger = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions());

    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService(
            _repository.Object,
            _passwordHasher.Object,
            _logger.Object,
            _tokenService.Object,
            _memoryCache,
            _emailService.Object);
    }

    [Fact]
    public async Task Register_ComDadosValidos_DeveCriarUsuario()
    {
        var dto = new RegisterDto
        {
            Name = "Maria",
            Email = "maria@teste.com",
            Password = "SenhaForte123!"
        };

        _repository
            .Setup(x => x.GetByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);

        _passwordHasher
            .Setup(x => x.HashPassword(
                It.IsAny<User>(),
                dto.Password))
            .Returns("hashed-password");

        await _service.Register(dto);

        _repository.Verify(
            x => x.AddAsync(It.Is<User>(u =>
                u.Name == dto.Name &&
                u.Email == dto.Email &&
                u.PasswordHash == "hashed-password" &&
                u.Active)),
            Times.Once);

        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task Register_EmailExistente_DeveLancarExcecao()
    {
        var dto = new RegisterDto
        {
            Name = "Maria",
            Email = "maria@teste.com",
            Password = "SenhaForte123!"
        };

        _repository
            .Setup(x => x.GetByEmailAsync(dto.Email))
            .ReturnsAsync(new User
            {
                Name = "Outra pessoa",
                Email = dto.Email,
                Active = true,
                PasswordHash = "hashed-password"
            });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.Register(dto));

        Assert.Equal("E-mail já cadastrado.", exception.Message);

        _repository.Verify(
            x => x.AddAsync(It.IsAny<User>()),
            Times.Never);

        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarToken()
    {
        var dto = new LoginDto
        {
            Email = "maria@teste.com",
            Password = "SenhaForte123!"
        };

        var user = new User
        {
            Name = "Maria",
            Email = dto.Email,
            PasswordHash = "hashed-password",
            Active = true
        };

        _repository
            .Setup(x => x.GetByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(x => x.VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.Password))
            .Returns(PasswordVerificationResult.Success);

        var expiresAt = DateTime.UtcNow.AddMinutes(30);

        _tokenService
            .Setup(x => x.GenerateToken(user))
            .Returns(("fake-jwt-token", expiresAt));

        var result = await _service.Login(dto);

        Assert.Equal("fake-jwt-token", result.Token);
        Assert.Equal(expiresAt, result.ExpiresAt);
        Assert.Equal(user.Email, result.User.Email);

        _tokenService.Verify(
            x => x.GenerateToken(user),
            Times.Once);
    }

    [Fact]
    public async Task Login_UsuarioInexistente_DeveLancarExcecao()
    {
        var dto = new LoginDto
        {
            Email = "inexistente@teste.com",
            Password = "SenhaForte123!"
        };

        _repository
            .Setup(x => x.GetByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.Login(dto));

        Assert.Equal(
            "E-mail ou senha inválidos.",
            exception.Message);

        _tokenService.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task Login_SenhaIncorreta_DeveLancarExcecao()
    {
        var dto = new LoginDto
        {
            Email = "maria@teste.com",
            Password = "SenhaErrada123!"
        };

        var user = new User
        {
            Name = "Maria",
            Email = dto.Email,
            PasswordHash = "hashed-password",
            Active = true
        };

        _repository
            .Setup(x => x.GetByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(x => x.VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.Password))
            .Returns(PasswordVerificationResult.Failed);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.Login(dto));

        _tokenService.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task Login_UsuarioInativo_DeveLancarExcecao()
    {
        var dto = new LoginDto
        {
            Email = "maria@teste.com",
            Password = "SenhaForte123!"
        };

        _repository
          .Setup(x => x.GetByEmailAsync(dto.Email))
            .ReturnsAsync(new User
            {
                Name = "Maria",
                Email = dto.Email,
                PasswordHash = string.Empty,
                Active = false
            });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.Login(dto));

        _passwordHasher.Verify(
            x => x.VerifyHashedPassword(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ForgotPassword_UsuarioInexistente_DeveLancarExcecao()
    {
        var dto = new ForgotPasswordDto
        {
            Email = "inexistente@teste.com"
        };

        _repository
            .Setup(x => x.GetByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ForgotPassword(dto));

        Assert.Equal("Usuário não encontrado.", exception.Message);

        _emailService.Verify(
            x => x.SendPasswordResetEmail(
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ResetPassword_CodigoInvalido_DeveLancarExcecao()
    {
        var dto = new PasswordResetDto
        {
            Code = "123456",
            NewPassword = "NovaSenha123!"
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ResetPassword(dto));

        Assert.Equal(
            "Código de redefinição de senha inválido ou expirado.",
            exception.Message);

        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
    }
}