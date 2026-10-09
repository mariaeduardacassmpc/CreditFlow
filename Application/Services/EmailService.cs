using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Application.Services;

public class EmailService(
    IConfiguration configuration,
    HttpClient httpClient
) : IEmailService
{
    public Task SendPasswordResetEmail(string email, string resetCode)
    {
        var html = $"""
            <h2>Redefinição de senha</h2>
            <p>Seu código de redefinição é:</p>
            <h1>{resetCode}</h1>
            <p>Este código é válido por 30 minutos.</p>
            <p>Se você não solicitou a redefinição, ignore este e-mail.</p>
            """;

        return SendEmailAsync(email, "Redefinição de senha - CreditFlow", html);
    }

    public Task SendTwoFactorCodeEmail(string email, string code)
    {
        var html = $"""
            <h2>Autenticação de dois fatores</h2>
            <p>Seu código para acessar o CreditFlow é:</p>
            <h1>{code}</h1>
            <p>Este código é válido por 5 minutos.</p>
            <p>Se você não tentou acessar sua conta, ignore este e-mail.</p>
            """;

        return SendEmailAsync(email, "Código de autenticação - CreditFlow", html);
    }

    private async Task SendEmailAsync(string email, string subject, string html)
    {
        var apiKey = configuration["AgentMail:ApiKey"];
        var inbox = configuration["AgentMail:Inbox"];

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(inbox))
        {
            throw new InvalidOperationException("Configure AgentMail:ApiKey e AgentMail:Inbox.");
        }

        var payload = new
        {
            to = new[] { email },
            subject,
            html
        };

        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"https://api.agentmail.to/v0/inboxes/{Uri.EscapeDataString(inbox)}/messages/send");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        using var response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            throw new InvalidOperationException($"Erro ao enviar e-mail: {error}");
        }
    }
}