using Application.Services;
using Domain.Entities;

namespace Application.Tests;

public class CreditAnalysisServiceTests
{
    [Fact]
    public void Analyze_WhenTermMonthsIsZero_ThrowsDivideByZeroException()
    {
        var service = new CreditAnalysisService();
        var creditRequest = CreateValidCreditRequest();
        creditRequest.TermMonths = 0;

        Assert.Throws<DivideByZeroException>(() => service.Analyze(creditRequest));
    }

    [Fact]
    public void Analyze_WhenScoreIsUnder600AndAmountIsAbove5000_ReprovesByScoreCompatibilityRule()
    {
        var service = new CreditAnalysisService();
        var creditRequest = CreateValidCreditRequest();
        creditRequest.CreditScore = 550;
        creditRequest.RequestedAmount = 6000;

        var result = service.Analyze(creditRequest);

        var scoreCompatibilityRule = result.Rules.Single(rule =>
            rule.Description == "Score compatível com o valor solicitado");

        Assert.Equal("Reprovado", result.Status);
        Assert.False(scoreCompatibilityRule.Approved);
    }

    private static CreditRequest CreateValidCreditRequest()
    {
        return new CreditRequest
        {
            CustomerId = 1,
            Customer = new Customer
            {
                CustomerId = 1,
                Name = "Test Customer",
                Phone = "11999999999",
                Email = "test@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                CreditScore = 700,
                Cpf = "12345678900"
            },
            RequestedAmount = 4000,
            MonthlyIncome = 4000,
            CreditScore = 700,
            EmploymentMonths = 24,
            TermMonths = 12,
            Purpose = CreditPurpose.Vehicle
        };
    }
}
