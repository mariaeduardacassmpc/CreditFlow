namespace Domain.Entities;

public class CreditRequest
{
    public int CreditRequestId { get; set; }

    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    public decimal RequestedAmount { get; set; }

    public decimal MonthlyIncome { get; set; }

    public CreditPurpose Purpose { get; set; }

    public int CreditScore { get; set; }

    public int EmploymentMonths { get; set; }

    public int TermMonths { get; set; }

    public string Status { get; set; } = "PENDING";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CreditAnalysis? CreditAnalysis { get; set; }
}