using System.Net.Http.Json;

namespace Infrastructure.ExternalServices;

public class CreditScoreProvider(HttpClient httpClient) : ICreditScoreProvider
{
    public async Task<int> GetScoreAsync(string email)
    {
        var response = await httpClient.GetAsync(
            $"api/CreditScore/{Uri.EscapeDataString(email)}");

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<CreditScoreResponse>();

        if (result is null)
            throw new InvalidOperationException(
                "Não foi possível obter o score de crédito.");

        return result.Score;
    }

    private class CreditScoreResponse
    {
        public int Score { get; set; }
    }
}