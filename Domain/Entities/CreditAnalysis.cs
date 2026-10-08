using Domain.Entities;

public class CreditAnalysis
{
    public int CreditAnalysisId { get; set; }

    public int CreditRequestId { get; set; }

    public CreditRequest CreditRequest { get; set; } = null!;

    public string Status { get; set; } = string.Empty;

    public DateTime AnalyzedAt { get; set; }

    public List<CreditRuleResult> Rules { get; set; } = [];
}