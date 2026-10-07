namespace Domain.Entities;

public class CreditRuleResult
{
    public int CreditRuleResultId { get; set; }

    public int CreditAnalysisId { get; set; }

    public CreditAnalysis CreditAnalysis { get; set; } = null!;

    public string Description { get; set; } = string.Empty;

    public bool Approved { get; set; }
}