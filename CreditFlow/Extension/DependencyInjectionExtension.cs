using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace CreditFlowAPI.NovaPasta;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<CustomerService>();
        services.AddScoped<AuthService>();

        services.AddHttpClient<IEmailService, EmailService>();

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        services.AddMemoryCache();

        return services;
    }
}
