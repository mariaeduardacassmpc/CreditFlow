using Application.Dtos;
using Application.Dtos.Credit;
using Application.Interfaces;
using Domain.Entities;
using Domain.Events;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class CreditRequestService(ICreditRequestRepository repository, ILogger<CreditRequestService> logger, ICreditScoreProvider creditScoreProvider, IKafkaProducer kafkaProducer)
{
    public async Task<CreditRequest> CreateCreditRequest(CreateCreditRequestDto dto)
    {
        logger.LogInformation(
            "Criando solicitação de crédito para o cliente {CustomerId}.",
            dto.CustomerId);

        var customer = await repository.GetCustomerAsync(dto.CustomerId);

        if (customer is null)
            throw new InvalidOperationException("Cliente não encontrado.");

        var creditRequest = new CreditRequest
        {
            CustomerId = dto.CustomerId,
            Customer = customer,
            RequestedAmount = dto.RequestedAmount,
            MonthlyIncome = dto.MonthlyIncome,
            CreditScore = customer.CreditScore,
            EmploymentMonths = dto.EmploymentMonths,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            Purpose = dto.Purpose,
            TermMonths = dto.TermMonths
        };

        var createdCreditRequest = await repository.CreateAsync(creditRequest);

        await kafkaProducer.PublishAsync(
            "credit-request-created",
            new CreditRequestCreatedEvent
            {
                CreditRequestId = createdCreditRequest.CreditRequestId
            });

        logger.LogInformation(
            "Solicitação de crédito {CreditRequestId} criada.",
            createdCreditRequest.CreditRequestId);

        return createdCreditRequest;
    }

    public async Task<IEnumerable<CreditRequestListDto>> GetAllCreditRequests()
    {
        logger.LogInformation(
            "Buscando todas solicitações de crédito.");

        var creditRequests = await repository.GetAllAsync();

        return creditRequests.Select(x => new CreditRequestListDto
        {
            CreditRequestId = x.CreditRequestId,
            CustomerId = x.CustomerId,
            RequestedAmount = x.RequestedAmount,
            CreatedAt = x.CreatedAt,
            Status = x.Status
        });
    }

    public async Task<CreditRequestDetailsDto?> GetById(int id)
    {
        logger.LogInformation(
            "Buscando solicitação de crédito por Id.");

        var creditRequest = await repository.GetByIdAsync(id);

        if (creditRequest is null)
            return null;

        return new CreditRequestDetailsDto
        {
            CreditRequestId = creditRequest.CreditRequestId,
            CustomerId = creditRequest.CustomerId,
            CustomerName = creditRequest.Customer.Name,
            CustomerEmail = creditRequest.Customer.Email,
            RequestedAmount = creditRequest.RequestedAmount,
            MonthlyIncome = creditRequest.MonthlyIncome,
            CreditScore = creditRequest.CreditScore,
            EmploymentMonths = creditRequest.EmploymentMonths,
            Status = creditRequest.Status,
            CreatedAt = creditRequest.CreatedAt
        };
    }
}