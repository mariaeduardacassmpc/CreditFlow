namespace Domain.Entities;

public class Customer
{
    public int CustomerId { get; set; }
    public required string Name { get; set; }
    public required string Phone { get; set; }
    public required string City { get; set; }
    public required string Address { get; set; }
    public bool Active { get; set; } = true;
    public required string Email { get; set; }
}

