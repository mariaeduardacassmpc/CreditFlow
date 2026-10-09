using Application.Dtos.Customers;
using Domain.Entities;

namespace Application.Interfaces;

public interface ICustomerService
{
    Task<Customer> CreateCustomer(CreateCustomerDto dto, CancellationToken cancellationToken);
    Task<IEnumerable<Customer>> GetAllCustomers(CancellationToken cancellationToken);
    Task<Customer?> GetById(int id, CancellationToken cancellationToken);
    Task<Customer?> UpdateCustomer(int id, UpdateCustomerDto dto, CancellationToken cancellationToken);
    Task<Customer?> ToggleActive(int id, CancellationToken cancellationToken);
}
