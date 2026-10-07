using Application.Dtos.Credit;
using Application.Dtos.CreditRequest;

namespace Application.Dtos.CreditRequest;

public class CreditAnalysisResult
{
    public string Status { get; set; } = string.Empty;

    public List<CreditRuleResultDto> Rules { get; set; } = [];
}