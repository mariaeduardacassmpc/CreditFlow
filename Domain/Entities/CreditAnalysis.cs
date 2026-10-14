using Domain.Entities;

public class CreditAnalysis
{
    public int CreditAnalysisId { get; set; }

    public int CreditRequestId { get; set; }

    public CreditRequest CreditRequest { get; set; } = null!;

    public Status Status { get; set; } 

    public DateTime AnalyzedAt { get; set; }

    public List<CreditRuleResult> Rules { get; set; } = new List<CreditRuleResult>();
}