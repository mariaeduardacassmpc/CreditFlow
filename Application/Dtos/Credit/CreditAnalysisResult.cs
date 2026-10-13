using Application.Dtos.Credit;
using Domain.Entities;

namespace Application.Dtos.CreditRequest;

public class CreditAnalysisResult
{
    public Status Status { get; set; }

    public List<CreditRuleResultDto> Rules { get; set; } = [];
}