using Application.Dtos.CreditRequest;
using Domain.Entities;

namespace Application.Interfaces;

public interface ICreditAnalysisService
{
    CreditAnalysisResult Analyze(CreditRequest creditRequest);
}
