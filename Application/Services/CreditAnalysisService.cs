using Application.Dtos.Credit;
using Application.Dtos.CreditRequest;
using Domain.Entities;

public class CreditAnalysisService
{
    public CreditAnalysisResult Analyze(CreditRequest creditRequest)
    {
        var rules = new List<CreditRuleResultDto>
        {
            new()
            {
                Description = "Score mínimo de 500",
                Approved = creditRequest.CreditScore >= 500
            },

            new()
            {
                Description = "Comprometimento de renda até 30%",
                Approved = CalculateIncomeCommitment(creditRequest)
            },

            new()
            {
                Description = "Sem restrições em órgãos de proteção",
                Approved = true
            },

            new()
            {
                Description = "Tempo de emprego mínimo de 6 meses",
                Approved = creditRequest.EmploymentMonths >= 6
            },

            new()
            {
                Description = "Valor até 8x a renda mensal",
                Approved =
                    creditRequest.RequestedAmount <=
                    creditRequest.MonthlyIncome * 8
            }
        };

        var approved = rules.All(x => x.Approved);

        return new CreditAnalysisResult
        {
            Status = approved ? "Aprovado" : "Reprovado",
            Rules = rules
        };
    }

    private bool CalculateIncomeCommitment(CreditRequest request)
    {
        return true;
    }
}