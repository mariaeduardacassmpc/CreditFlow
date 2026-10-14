using Application.Dtos.Dashboard;

namespace Application.Interfaces;

public interface IDashboardService
{
    Task<CreditDashboardDto> GetDashboard();
}
