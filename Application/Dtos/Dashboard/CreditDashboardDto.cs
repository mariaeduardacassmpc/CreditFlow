namespace Application.Dtos.Dashboard;

public class CreditDashboardDto
{
    public int TotalRequests { get; set; }

    public int RequestsInAnalysis { get; set; }

    public int ApprovedRequests { get; set; }

    public int RejectedRequests { get; set; }
}