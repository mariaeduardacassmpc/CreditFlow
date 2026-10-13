using Application.Common;
using Application.Dtos.Customers;

namespace Application.Interfaces;

public interface ICustomerService
{
    Task<CustomerResponseDto> CreateCustomer(CreateCustomerDto dto, CancellationToken cancellationToken);
    Task<PagedResult<CustomerResponseDto>> GetAllCustomers(int page, int pageSize, CancellationToken cancellationToken);
    Task<CustomerResponseDto?> GetById(int id, CancellationToken cancellationToken);
    Task<CustomerResponseDto?> UpdateCustomer(int id, UpdateCustomerDto dto, CancellationToken cancellationToken);
    Task<CustomerResponseDto?> SetActive(int id, bool active, CancellationToken cancellationToken);
}