namespace Domain.Entities;

public class Customer
{
    public int CustomerId { get; set; }
    public required string Name { get; set; }
    public required string Phone { get; set; }
    public bool Active { get; set; } = true;
    public required string Email { get; set; }
    public required DateTime BirthDate { get; set; }
    public int CreditScore { get; set; }
    public required string Cpf { get; set; } = string.Empty;
}

