using Application.Dtos.Auth;
using Application.Dtos.Password;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CreditFlowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IAuthService authService,
    IPasswordResetService passwordResetService,
    ITwoFactorAuthService twoFactorAuthService
) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto, CancellationToken cancellation)
    {
        await authService.Register(dto, cancellation);

        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto, CancellationToken cancellation)
    {
        var response = await authService.Login(dto, cancellation);

        return Ok(response);
    }

    [HttpPost("verify-2fa")]
    public async Task<IActionResult> VerifyTwoFactor(VerifyTwoFactorDto dto, CancellationToken cancellation)
    {
        var response = await twoFactorAuthService.VerifyCode(dto, cancellation);

        return Ok(response);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto, CancellationToken cancellation)
    {
        await passwordResetService.ForgotPassword(dto, cancellation);

        return Ok();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(PasswordResetDto dto, CancellationToken cancellation)
    {
        await passwordResetService.ResetPassword(dto, cancellation);

        return Ok();
    }
}