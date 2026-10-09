using Application.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text;

namespace CreditFlow.Tests.Services;

public class EmailServiceTests
{
    private static IConfiguration CreateConfiguration(string? apiKey = "test-api-key", string? inbox = "test-inbox")
    {
        var settings = new Dictionary<string, string?> { ["AgentMail:ApiKey"] = apiKey, ["AgentMail:Inbox"] = inbox };
        return new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
    }

    private static HttpClient CreateHttpClient(HttpStatusCode statusCode, string responseBody = "")
    {
        var handler = new MockHttpMessageHandler((request, cancellationToken) => Task.FromResult(new HttpResponseMessage(statusCode) { Content = new StringContent(responseBody, Encoding.UTF8, "application/json") }));
        return new HttpClient(handler);
    }

    [Fact]
    public async Task SendPasswordResetEmail_ComConfiguracaoValida_DeveEnviarRequisicao()
    {
        var configuration = CreateConfiguration();
        var httpClient = CreateHttpClient(HttpStatusCode.OK);
        var service = new EmailService(configuration, httpClient);

        await service.SendPasswordResetEmail("maria@teste.com", "123456");
    }

    [Fact]
    public async Task SendPasswordResetEmail_SemApiKey_DeveLancarExcecao()
    {
        var configuration = CreateConfiguration(apiKey: null);
        var service = new EmailService(configuration, CreateHttpClient(HttpStatusCode.OK));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.SendPasswordResetEmail("maria@teste.com", "123456"));

        Assert.Equal("A chave da API do AgentMail não foi configurada.", exception.Message);
    }

    [Fact]
    public async Task SendPasswordResetEmail_SemInbox_DeveLancarExcecao()
    {
        var configuration = CreateConfiguration(inbox: null);
        var service = new EmailService(configuration, CreateHttpClient(HttpStatusCode.OK));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.SendPasswordResetEmail("maria@teste.com", "123456"));

        Assert.Equal("O inbox do AgentMail não foi configurado.", exception.Message);
    }

    [Fact]
    public async Task SendPasswordResetEmail_RespostaComErro_DeveLancarExcecao()
    {
        var configuration = CreateConfiguration();
        var httpClient = CreateHttpClient(HttpStatusCode.BadRequest, "Erro de teste");
        var service = new EmailService(configuration, httpClient);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.SendPasswordResetEmail("maria@teste.com", "123456"));

        Assert.Equal("Erro ao enviar e-mail: Erro de teste", exception.Message);
    }

    private sealed class MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => sendAsync(request, cancellationToken);
    }
}