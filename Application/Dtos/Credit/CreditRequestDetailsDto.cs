namespace Application.Dtos;

public class CreditRequestDetailsDto
{
	public int CreditRequestId { get; set; }
	public int CustomerId { get; set; }
	public string CustomerName { get; set; } = string.Empty;
	public string CustomerEmail { get; set; } = string.Empty;
	public decimal RequestedAmount { get; set; }
	public decimal MonthlyIncome { get; set; }
	public int CreditScore { get; set; }
	public int EmploymentMonths { get; set; }
	public string Status { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; }
}
