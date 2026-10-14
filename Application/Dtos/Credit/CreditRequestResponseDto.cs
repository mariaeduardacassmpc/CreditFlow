namespace Application.Dtos.Credit;

public class CreditRequestResponseDto
{
    public int CreditRequestId { get; set; }
    public int CustomerId { get; set; }
    public decimal RequestedAmount { get; set; }
    public decimal MonthlyIncome { get; set; }
    public int CreditScore { get; set; }
    public int EmploymentMonths { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public int TermMonths { get; set; }
}