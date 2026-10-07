using Domain.Entities;

namespace Application.Interfaces;

public interface ICustomerRepository
{
    Task<Customer> CreateAsync(Customer customer);

    Task<IEnumerable<Customer>> GetAllAsync();

    Task<Customer?> GetByIdAsync(int id);

    Task<Customer?> UpdateAsync(int id, Customer customer);
}