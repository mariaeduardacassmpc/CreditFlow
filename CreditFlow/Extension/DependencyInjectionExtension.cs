using Application.Interfaces;
using Application.Services;
using Application.Validators;
using Domain.Entities;
using Infrastructure;
using Infrastructure.ExternalServices;
using Infrastructure.Messaging;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace CreditFlowAPI.Extension;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<CustomerService>();
        services.AddScoped<DashboardService>();
        services.AddScoped<CreditAnalysisService>();
        services.AddScoped<CreditRequestService>();
        services.AddScoped<ITwoFactorAuthService, TwoFactorAuthService>();
        services.AddScoped<IPasswordResetService, PasswordResetService>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICreditRequestRepository, CreditRequestRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<CreditAnalysisService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CreateCustomerValidator>();
        services.AddHostedService<KafkaConsumer>(); 
        services.AddHostedService<KafkaEmailConsumer>();

        services.AddSingleton<IKafkaProducer, KafkaProducer>();
        services.AddHttpClient<IEmailService, EmailService>();
        services.AddHttpClient<ICreditScoreProvider, CreditScoreProvider>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5171/");
        });

        services.AddMemoryCache();

        return services;
    }
}