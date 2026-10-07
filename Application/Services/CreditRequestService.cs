using Application.Dtos.Auth;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class CreditRequestService(CreditFlowDbContext context)
{
    public async Task<CreditRequest> CreateCreditRequest(CreateCreditRequestDto dto)
    {
        var customer = await context.Customers
            .FindAsync(dto.CustomerId);

        if (customer is null)
            throw new InvalidOperationException("Cliente não encontrado.");

        var creditRequest = new CreditRequest
        {
            CustomerId = dto.CustomerId,
            RequestedAmount = dto.RequestedAmount,
            MonthlyIncome = dto.MonthlyIncome,
            CreditScore = dto.CreditScore,
            EmploymentMonths = dto.EmploymentMonths,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        context.CreditRequests.Add(creditRequest);

        await context.SaveChangesAsync();

        return creditRequest;
    }

    public async Task<IEnumerable<CreditRequest>> GetAllCreditRequests()
    {
        return await context.CreditRequests.ToListAsync();
    }

    public async Task<CreditRequest?> GetById(int id)
    {
        return await context.CreditRequests.FindAsync(id);
    }
}