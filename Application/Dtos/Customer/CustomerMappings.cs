using Domain.Entities;

namespace Application.Dtos.Customers;

public static class CustomerMappings
{
    public static CustomerResponseDto ToResponse(this Customer customer) => new(
        customer.CustomerId,
        customer.Name,
        customer.Phone,
        customer.Email,
        MaskCpf(customer.Cpf),
        customer.BirthDate,
        customer.CreditScore,
        customer.Active);

    private static string MaskCpf(string cpf) =>
        cpf.Length == 11 ? $"***.{cpf[3..6]}.{cpf[6..9]}-**" : "***";
}