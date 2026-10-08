using Application.Dtos.Credit;
using Application.Dtos.CreditRequest;
using Domain.Entities;

public class CreditAnalysisService
{
    public CreditAnalysisResult Analyze(CreditRequest creditRequest)
    {
        var riskLevel = CalculateRiskLevel(creditRequest);
        var age = CalculateAge(creditRequest.Customer.BirthDate,DateTime.UtcNow);
        var ageAtEndOfContract = CalculateAge(creditRequest.Customer.BirthDate, DateTime.UtcNow.AddMonths(creditRequest.TermMonths));

        var rules = new List<CreditRuleResultDto>
        {
            new()
            {
                Description = "Renda mensal mínima de R$ 1.500",
                Approved = creditRequest.MonthlyIncome >= 1500
            },
            new()
            {
                Description = "Cliente não apresenta perfil de alto risco",
                Approved = riskLevel != RiskLevel.High
            },
            new()
            {
                Description = "Idade entre 18 e 70 anos",
                Approved = age >= 18 && age <= 70
            },
            new()
            {
                Description = "Idade ao final do contrato não ultrapassa 75 anos",
                Approved = ageAtEndOfContract <= 75
            },
            new()
            {
                Description = "Valor solicitado mínimo de R$ 500",
                Approved = creditRequest.RequestedAmount >= 500
            },
            new()
            {
                Description = "Score mínimo de 500",
                Approved = creditRequest.CreditScore >= 500
            },
            new()
            {
                Description = "Score compatível com o valor solicitado",
                Approved = IsScoreCompatibleWithAmount(creditRequest)
            },
            new()
            {
                Description = "Comprometimento de renda até 30%",
                Approved = CalculateIncomeCommitment(creditRequest)
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
            },
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
        var estimatedInstallment =
            request.RequestedAmount / request.TermMonths;

        return estimatedInstallment <=
               request.MonthlyIncome * 0.30m;
    }

    private RiskLevel CalculateRiskLevel(CreditRequest creditRequest)
    {
        var riskPoints = 0;

        if (creditRequest.CreditScore < 600)
            riskPoints += 3;
        else if (creditRequest.CreditScore < 700)
            riskPoints += 1;

        if (creditRequest.EmploymentMonths < 6)
            riskPoints += 3;
        else if (creditRequest.EmploymentMonths < 12)
            riskPoints += 1;

        if (creditRequest.RequestedAmount >
            creditRequest.MonthlyIncome * 8)
            riskPoints += 3;

        if (!CalculateIncomeCommitment(creditRequest))
            riskPoints += 2;

        return riskPoints switch
        {
            var x when x <= 2 => RiskLevel.Low,
            var x when x <= 5 => RiskLevel.Medium,
            _ => RiskLevel.High
        };
    }

    private bool IsScoreCompatibleWithAmount(CreditRequest request)
    {
        return request.CreditScore switch
        {
            < 600 => request.RequestedAmount <= 5000,
            < 700 => request.RequestedAmount <= 15000,
            < 750 => request.RequestedAmount <= 30000,
            _ => true
        };
    }

    private int CalculateAge(DateTime birthDate, DateTime referenceDate)
    {
        var age = referenceDate.Year - birthDate.Year;

        if (referenceDate.Date < birthDate.Date.AddYears(age))
            age--;

        return age;
    }
}