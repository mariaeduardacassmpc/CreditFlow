using Application.Dtos.Dashboard;
using Application.Interfaces;

namespace Application.Services;

public class DashboardService(IDashboardRepository repository)
{
    public async Task<CreditDashboardDto> GetDashboard()
    {
        var totalRequests = await repository.GetTotalRequestsAsync();

        var requestsInAnalysis = await repository.GetRequestsInAnalysisAsync();

        var approvedRequests = await repository.GetApprovedRequestsAsync();

        var rejectedRequests = await repository.GetRejectedRequestsAsync();

        return new CreditDashboardDto
        {
            TotalRequests = totalRequests,
            RequestsInAnalysis = requestsInAnalysis,
            ApprovedRequests = approvedRequests,
            RejectedRequests = rejectedRequests
        };
    }
}