namespace Application.Dtos.Customers;

public class UpdateCustomerDto
{
    public required string Name { get; set; }
    public required string Phone { get; set; }
    public required string Email { get; set; }
    public DateTime? BirthDate { get; set; }
    public required string Cpf { get; set; }
}