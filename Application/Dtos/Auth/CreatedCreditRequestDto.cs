namespace Application.Dtos.Auth;
public class CreateCreditRequestDto
{
    public int CustomerId { get; set; }
    public decimal RequestedAmount { get; set; }
    public decimal MonthlyIncome { get; set; }
    public int CreditScore { get; set; }
    public int EmploymentMonths { get; set; }
}