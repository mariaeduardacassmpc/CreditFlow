using Application.Common;
using Domain.Entities;

namespace Application.Interfaces;

public interface ICreditRequestRepository
{
    Task<Customer?> GetCustomerAsync(int customerId, CancellationToken cancellationToken);
    Task<CreditRequest> CreateAsync(CreditRequest creditRequest, CancellationToken cancellationToken);
    Task<PagedResult<CreditRequest>> GetAllAsync(int page, int pageSize, int? customerId, Status? status, CancellationToken cancellationToken);
    Task<CreditRequest?> GetByIdAsync(int id, CancellationToken cancellationToken);
}