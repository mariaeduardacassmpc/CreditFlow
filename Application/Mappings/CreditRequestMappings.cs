using Application.Dtos.Credit;
using Application.Dtos;
using Domain.Entities;

namespace Application.Mappings;

public static class CreditRequestMappings
{
    public static CreditRequestResponseDto ToResponseDto(
        this CreditRequest creditRequest)
    {
        return new CreditRequestResponseDto
        {
            CreditRequestId = creditRequest.CreditRequestId,
            CustomerId = creditRequest.CustomerId,
            RequestedAmount = creditRequest.RequestedAmount,
            MonthlyIncome = creditRequest.MonthlyIncome,
            CreditScore = creditRequest.CreditScore,
            EmploymentMonths = creditRequest.EmploymentMonths,
            Status = creditRequest.Status.ToString(),
            CreatedAt = creditRequest.CreatedAt,
            Purpose = creditRequest.Purpose.ToString(),
            TermMonths = creditRequest.TermMonths
        };
    }

    public static CreditRequestListDto ToListDto(
        this CreditRequest creditRequest)
    {
        return new CreditRequestListDto
        {
            CreditRequestId = creditRequest.CreditRequestId,
            CustomerId = creditRequest.CustomerId,
            RequestedAmount = creditRequest.RequestedAmount,
            CreatedAt = creditRequest.CreatedAt,
            Status = creditRequest.Status
        };
    }

    public static CreditRequestDetailsDto ToDetailsDto(
        this CreditRequest creditRequest)
    {
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
