using Application.Dtos.Auth;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace CreditFlow.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _repository = new();
    private readonly Mock<IPasswordHasher<User>> _passwordHasher = new();
    private readonly Mock<ILogger<AuthService>> _logger = new();
    private readonly Mock<ITwoFactorAuthService> _twoFactorAuthService = new();

    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService(
            _repository.Object,
            _passwordHasher.Object,
            _logger.Object,
            _twoFactorAuthService.Object);
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

        await _service.Register(dto, _cancellationToken);

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
            () => _service.Register(dto, _cancellationToken));

        Assert.Equal("E-mail já cadastrado.", exception.Message);

        _repository.Verify(
            x => x.AddAsync(It.IsAny<User>()),
            Times.Never);

        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveCriarDesafio()
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

        var expectedResponse = new LoginChallengeResponseDto
        {
            Message = "Código de verificação enviado.",
            ChallengeId = Guid.NewGuid().ToString()
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

        _twoFactorAuthService
            .Setup(x => x.CreateChallenge(user, _cancellationToken))
            .ReturnsAsync(expectedResponse);

        var result = await _service.Login(dto, _cancellationToken);

        Assert.Same(expectedResponse, result);

        _twoFactorAuthService.Verify(
            x => x.CreateChallenge(user, _cancellationToken),
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
            () => _service.Login(dto, _cancellationToken));

        Assert.Equal("E-mail ou senha inválidos.", exception.Message);

        _twoFactorAuthService.Verify(
            x => x.CreateChallenge(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
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
            () => _service.Login(dto, _cancellationToken));

        _twoFactorAuthService.Verify(
            x => x.CreateChallenge(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
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
            () => _service.Login(dto, _cancellationToken));

        _passwordHasher.Verify(
            x => x.VerifyHashedPassword(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);

        _twoFactorAuthService.Verify(
            x => x.CreateChallenge(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}