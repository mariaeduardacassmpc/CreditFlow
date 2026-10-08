namespace Application.Interfaces;

public interface ICreditScoreProvider
{
    Task<int> GetScoreAsync(string cpf);
}