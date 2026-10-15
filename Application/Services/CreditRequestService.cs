using Application.Common;
using Application.Dtos;
using Application.Dtos.Credit;
using Application.Interfaces;
using Application.Mappings;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class CreditRequestService(
    ICreditRequestRepository repository,
    ILogger<CreditRequestService> logger
) : ICreditRequestService
{
    public async Task<CreditRequestResponseDto> CreateCreditRequest(CreateCreditRequestDto dto, CancellationToken cancellationToken)
    {
        logger.LogInformation("Criando solicitação de crédito para o cliente {CustomerId}.", dto.CustomerId);

        var customer = await repository.GetCustomerAsync(dto.CustomerId, cancellationToken);

        if (customer is null)
            throw new InvalidOperationException("Cliente não encontrado.");

        if (!customer.Active)
            throw new InvalidOperationException("O cliente está inativo.");

        var creditRequest = new CreditRequest
        {
            CustomerId = customer.CustomerId,
            Customer = customer,
            RequestedAmount = dto.RequestedAmount,
            MonthlyIncome = dto.MonthlyIncome,
            CreditScore = customer.CreditScore,
            EmploymentMonths = dto.EmploymentMonths,
            Status = Status.Pending,
            CreatedAt = DateTime.UtcNow,
            Purpose = dto.Purpose,
            TermMonths = dto.TermMonths
        };

        var createdCreditRequest = await repository.CreateAsync(creditRequest, cancellationToken);

        logger.LogInformation("Solicitação de crédito {CreditRequestId} criada para o cliente {CustomerId}.", createdCreditRequest.CreditRequestId, createdCreditRequest.CustomerId);

        return createdCreditRequest.ToResponseDto();
    }

    public async Task<PagedResult<CreditRequestListDto>> GetAllCreditRequests(int page, int pageSize, int? customerId, Status? status, CancellationToken cancellationToken)
    {
        logger.LogInformation("Buscando solicitações de crédito. Página: {Page}, Tamanho: {PageSize}, Cliente: {CustomerId}, Status: {Status}", page, pageSize, customerId, status);

        var result = await repository.GetAllAsync(page, pageSize, customerId, status, cancellationToken);

        var items = result.Items.Select(x => x.ToListDto()).ToList();

        return new PagedResult<CreditRequestListDto>(items, page, pageSize, result.TotalCount);
    }

    public async Task<CreditRequestDetailsDto?> GetById(int id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Buscando solicitação de crédito por Id: {CreditRequestId}.", id);

        var creditRequest = await repository.GetByIdAsync(id, cancellationToken);

        if (creditRequest is null)
            return null;

        return creditRequest.ToDetailsDto();
    }
}