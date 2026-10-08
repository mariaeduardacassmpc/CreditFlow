using Domain.Entities;

namespace Application.Interfaces;

public interface ICreditRequestRepository
{
    Task<Customer?> GetCustomerAsync(int customerId);
    Task<CreditRequest> CreateAsync(CreditRequest creditRequest);
    Task<IEnumerable<CreditRequest>> GetAllAsync();
    Task<CreditRequest?> GetByIdAsync(int id);
}