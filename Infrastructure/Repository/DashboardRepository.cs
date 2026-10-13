using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DashboardRepository(CreditFlowDbContext context) : IDashboardRepository
{
    public async Task<int> GetTotalRequestsAsync()
    {
        return await context.CreditRequests.CountAsync();
    }

    public async Task<int> GetRequestsInAnalysisAsync()
    {
        return await context.CreditRequests.CountAsync(x => x.Status == Status.Pending);
    }

    public async Task<int> GetApprovedRequestsAsync()
    {
        return await context.CreditRequests.CountAsync(x => x.Status == Status.Approved);
    }

    public async Task<int> GetRejectedRequestsAsync()
    {
        return await context.CreditRequests.CountAsync(x => x.Status == Status.Rejected);
    }
}