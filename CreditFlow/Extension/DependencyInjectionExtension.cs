using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Infrastructure;
using Infrastructure.ExternalServices;
using Microsoft.AspNetCore.Identity;

namespace CreditFlowAPI.NovaPasta;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<CustomerService>();
        services.AddScoped<AuthService>();
        services.AddScoped<CreditRequestService>();
        services.AddScoped<DashboardService>();
        services.AddHttpClient<IEmailService, EmailService>();
        services.AddHttpClient<CreditScoreProvider>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5171/");
        });

        services.AddScoped<ICreditScoreProvider>(sp => sp.GetRequiredService<CreditScoreProvider>());
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        services.AddMemoryCache();

        return services;
    }
}
