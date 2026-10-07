using Application.Dtos;
using Application.Dtos.Credit;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.ExternalServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class CreditRequestService(CreditFlowDbContext context, ILogger<CreditRequestService> logger, ICreditScoreProvider creditScoreProvider)
{
    public async Task<CreditRequest> CreateCreditRequest(CreateCreditRequestDto dto)
    {
        logger.LogInformation("Criando solicitação de crédito para o cliente {CustomerId}.", dto.CustomerId);

        var customer = await context.Customers
            .FindAsync(dto.CustomerId);

        if (customer is null)
            throw new InvalidOperationException("Cliente não encontrado.");

        var creditScore = await creditScoreProvider
            .GetScoreAsync(customer.Email);

        var creditRequest = new CreditRequest
        {
            CustomerId = dto.CustomerId,
            RequestedAmount = dto.RequestedAmount,
            MonthlyIncome = dto.MonthlyIncome,
            CreditScore = creditScore,
            EmploymentMonths = dto.EmploymentMonths,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            Purpose = dto.Purpose
        };

        context.CreditRequests.Add(creditRequest);

        await context.SaveChangesAsync();

        logger.LogInformation(
            "Solicitação de crédito {CreditRequestId} criada com score {CreditScore}.",
            creditRequest.CreditRequestId,
            creditScore);

        return creditRequest;
    }

    public async Task<IEnumerable<CreditRequestListDto>> GetAllCreditRequests()
    {
        logger.LogInformation("Buscando todas solicitações de crédito.");

        return await context.CreditRequests
            .Select(x => new CreditRequestListDto
            {
                CreditRequestId = x.CreditRequestId,
                CustomerId = x.CustomerId,
                RequestedAmount = x.RequestedAmount,
                CreatedAt = x.CreatedAt,
                Status = x.Status
            })
            .ToListAsync();
    }

    public async Task<CreditRequestDetailsDto?> GetById(int id)
    {
        logger.LogInformation("Buscando solicitação de crédito por Id.");

        return await context.CreditRequests
            .Where(x => x.CreditRequestId == id)
            .Select(x => new CreditRequestDetailsDto
            {
                CreditRequestId = x.CreditRequestId,
                CustomerId = x.CustomerId,
                CustomerName = x.Customer.Name,
                CustomerEmail = x.Customer.Email,
                RequestedAmount = x.RequestedAmount,
                MonthlyIncome = x.MonthlyIncome,
                CreditScore = x.CreditScore,
                EmploymentMonths = x.EmploymentMonths,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();
    }
}