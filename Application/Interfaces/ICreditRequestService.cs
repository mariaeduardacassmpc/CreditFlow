using Application.Common;
using Application.Dtos;
using Application.Dtos.Credit;
using Domain.Entities;

namespace Application.Interfaces;

public interface ICreditRequestService
{
    Task<CreditRequest> CreateCreditRequest(CreateCreditRequestDto dto, CancellationToken cancellationToken);
    Task<PagedResult<CreditRequestListDto>> GetAllCreditRequests(int page, int pageSize, int? customerId, Status? status, CancellationToken cancellationToken);
    Task<CreditRequestDetailsDto?> GetById(int id, CancellationToken cancellationToken);
}