using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class CreditRequestService(ICreditRequestRepository creditRequestRepository)
{
    public async Task<CreditRequest> CreateCreditRequest(CreditRequest creditRequest)
    {
        creditRequest.Status = "PENDING";
        creditRequest.CreatedAt = DateTime.UtcNow;

        return await creditRequestRepository.CreateAsync(creditRequest);
    }

    public async Task<IEnumerable<CreditRequest>> GetAllCreditRequests()
    {
        return await creditRequestRepository.GetAllAsync();
    }

    public async Task<CreditRequest?> GetById(int id)
    {
        return await creditRequestRepository.GetByIdAsync(id);
    }
}