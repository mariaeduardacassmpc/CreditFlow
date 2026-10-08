namespace Application.Interfaces;

public interface IDashboardRepository
{
    Task<int> GetTotalRequestsAsync();
    Task<int> GetRequestsInAnalysisAsync();
    Task<int> GetApprovedRequestsAsync();
    Task<int> GetRejectedRequestsAsync();
}