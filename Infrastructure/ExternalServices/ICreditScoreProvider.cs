namespace Infrastructure.ExternalServices;

public interface ICreditScoreProvider
{
    Task<int> GetScoreAsync(string cpf);
}