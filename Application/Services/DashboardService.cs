using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Application.Dtos.Dashboard;

namespace Application.Services;

public class DashboardService(CreditFlowDbContext context)
{
    public async Task<CreditDashboardDto> GetDashboard()
    {
        var totalRequests = await context.CreditRequests
            .CountAsync();

        var requestsInAnalysis = await context.CreditRequests
            .CountAsync(x => x.Status == "Pending");

        var approvedRequests = await context.CreditRequests
            .CountAsync(x => x.Status == "Aprovado");

        var rejectedRequests = await context.CreditRequests
            .CountAsync(x => x.Status == "Reprovado");

        return new CreditDashboardDto
        {
            TotalRequests = totalRequests,
            RequestsInAnalysis = requestsInAnalysis,
            ApprovedRequests = approvedRequests,
            RejectedRequests = rejectedRequests
        };
    }
}