using Application.Dtos.Auth;
using Application.Dtos.Password;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CreditFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IAuthService authService,
    IPasswordResetService passwordResetService,
    ITwoFactorAuthService twoFactorAuthService
) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        await authService.Register(dto);

        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var response = await authService.Login(dto);

        return Ok(response);
    }

    [HttpPost("verify-2fa")]
    public async Task<IActionResult> VerifyTwoFactor(VerifyTwoFactorDto dto)
    {
        var response = await twoFactorAuthService.VerifyCode(dto);

        return Ok(response);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        await passwordResetService.ForgotPassword(dto);

        return Ok();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(PasswordResetDto dto)
    {
        await passwordResetService.ResetPassword(dto);

        return Ok();
    }
}