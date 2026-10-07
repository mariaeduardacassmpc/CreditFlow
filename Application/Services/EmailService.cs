using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Application.Services;
public class EmailService(IConfiguration configuration, HttpClient httpClient) : IEmailService
{
    public async Task SendPasswordResetEmail(string email, string resetCode)
    {
        var apiKey = configuration["AgentMail:ApiKey"];
        var inbox = configuration["AgentMail:Inbox"];

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException(
                "A chave da API do AgentMail não foi configurada."
            );

        if (string.IsNullOrWhiteSpace(inbox))
            throw new InvalidOperationException(
                "O inbox do AgentMail não foi configurado."
            );

        var resetUrl =
            $"https://adm-doces-e-cia.vercel.app/reset-password" +
            $"?email={Uri.EscapeDataString(email)}" +
            $"&token={Uri.EscapeDataString(resetCode)}";

        var payload = new
        {
            to = new[] { email },
            subject = "Redefinição de senha",
            text = $"""
                Recebemos uma solicitação para redefinir sua senha.
                Seu código de redefinição é: {resetCode}
                Este código é válido por 30 minutos.

                Caso você não tenha solicitado a redefinição de senha, ignore este e-mail.
                """,
               html = $"""
                <html>
                    <body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
                        <h2>Redefinição de senha</h2>

                        <p>
                            Olá! Recebemos uma solicitação para redefinir sua senha.
                        </p>

                        <p>
                            Seu código de redefinição de senha é:
                        </p>

                        <p style=" font-size: 24px; font-weight: bold; letter-spacing: 4px; margin: 16px 0; "> 
                            {resetCode} 
                        </p>
                            Este código é válido por <strong>30 minutos</strong>.
                        </p>

                        <p>
                            Caso você não tenha solicitado a redefinição de senha,
                            ignore este e-mail.
                        </p>
                    </body>
                </html>
                """

        };

        var json = JsonSerializer.Serialize(payload);

        using var request = new HttpRequestMessage(
            HttpMethod.Post, $"https://api.agentmail.to/v0/inboxes/{Uri.EscapeDataString(inbox)}/messages/send");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        request.Content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json"
        );

        var response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            throw new InvalidOperationException(
                $"Erro ao enviar e-mail: {error}"
            );
        }
    }
}