namespace Application.Dtos.Customers;

public record CustomerResponseDto(
    int CustomerId,
    string Name,
    string Phone,
    string Email,
    string MaskedCpf,
    DateOnly BirthDate,
    int CreditScore,
    bool Active
);