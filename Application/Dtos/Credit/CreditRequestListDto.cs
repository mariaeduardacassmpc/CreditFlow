using Domain.Entities;

namespace Application.Dtos.Credit;

public class CreditRequestListDto
{
    public int CreditRequestId { get; set; }
    public int CustomerId { get; set; }
    public decimal RequestedAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public Status Status { get; set; } 
}