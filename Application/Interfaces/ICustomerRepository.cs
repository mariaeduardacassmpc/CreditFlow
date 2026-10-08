using Domain.Entities;

namespace Application.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id);
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer?> GetByEmailAsync(string email);
    Task AddAsync(Customer customer);
    Task SaveChangesAsync();
}