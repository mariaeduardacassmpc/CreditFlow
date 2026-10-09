using Application.Dtos.Credit;
using Application.Dtos.CreditRequest;
using Domain.Entities;
using Xunit;

namespace CreditFlow.Tests;

public class CreditAnalysisServiceTests
{
    private readonly CreditAnalysisService _service = new();

    private static CreditRequest CreateValidRequest()
    {
        return new CreditRequest
        {
            CustomerId = 1,
            Customer = new Customer
            {
                CustomerId = 1,
                Name = "Maria Silva",
                Email = "maria@email.com",
                Cpf = "12345678900",
                Phone = "43999999999",
                Active = true,
                BirthDate = new DateTime(1995, 1, 1),
                CreditScore = 800
            },
            RequestedAmount = 5000m,
            MonthlyIncome = 3000m,
            CreditScore = 800,
            EmploymentMonths = 24,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            Purpose = CreditPurpose.Business,
            TermMonths = 24
        };
    }

    [Fact]
    public void Analyze_WhenAllRulesAreMet_ShouldApprove()
    {
        var request = CreateValidRequest();

        var result = _service.Analyze(request);

        Assert.Equal("Aprovado", result.Status);
        Assert.All(result.Rules, rule => Assert.True(rule.Approved));
    }

    [Fact]
    public void Analyze_WhenMonthlyIncomeIsBelowMinimum_ShouldRejectRule()
    {
        var request = CreateValidRequest();
        request.MonthlyIncome = 1000m;

        var result = _service.Analyze(request);

        AssertRuleRejected(result, "Renda mensal mínima de R$ 1.500");
        Assert.Equal("Reprovado", result.Status);
    }

    [Fact]
    public void Analyze_WhenCreditScoreIsBelow500_ShouldRejectScoreRule()
    {
        var request = CreateValidRequest();
        request.CreditScore = 450;
        request.Customer.CreditScore = 450;

        var result = _service.Analyze(request);

        AssertRuleRejected(result, "Score mínimo de 500");
    }

    [Fact]
    public void Analyze_WhenRequestedAmountIsBelow500_ShouldRejectAmountRule()
    {
        var request = CreateValidRequest();
        request.RequestedAmount = 400m;

        var result = _service.Analyze(request);

        AssertRuleRejected(result, "Valor solicitado mínimo de R$ 500");
    }

    [Fact]
    public void Analyze_WhenEmploymentIsBelow6Months_ShouldRejectEmploymentRule()
    {
        var request = CreateValidRequest();
        request.EmploymentMonths = 3;

        var result = _service.Analyze(request);

        AssertRuleRejected(result, "Tempo de emprego mínimo de 6 meses");
    }

    [Fact]
    public void Analyze_WhenAmountExceedsEightTimesIncome_ShouldRejectAmountToIncomeRule()
    {
        var request = CreateValidRequest();
        request.RequestedAmount = 25000m;
        request.MonthlyIncome = 3000m;

        var result = _service.Analyze(request);

        AssertRuleRejected(result, "Valor até 8x a renda mensal");
    }

    [Fact]
    public void Analyze_WhenIncomeCommitmentExceeds30Percent_ShouldRejectRule()
    {
        var request = CreateValidRequest();
        request.RequestedAmount = 20000m;
        request.TermMonths = 12;
        request.MonthlyIncome = 3000m;

        var result = _service.Analyze(request);

        AssertRuleRejected(result, "Comprometimento de renda até 30%");
    }

    [Fact]
    public void Analyze_WhenScoreIsNotCompatibleWithAmount_ShouldRejectRule()
    {
        var request = CreateValidRequest();
        request.CreditScore = 550;
        request.Customer.CreditScore = 550;
        request.RequestedAmount = 6000m;

        var result = _service.Analyze(request);

        AssertRuleRejected(result, "Score compatível com o valor solicitado");
    }

    [Fact]
    public void Analyze_WhenCustomerIsUnder18_ShouldRejectAgeRule()
    {
        var request = CreateValidRequest();
        request.Customer.BirthDate = DateTime.UtcNow.AddYears(-17);

        var result = _service.Analyze(request);

        AssertRuleRejected(result, "Idade entre 18 e 70 anos");
    }

    [Fact]
    public void Analyze_WhenCustomerIsOver70_ShouldRejectAgeRule()
    {
        var request = CreateValidRequest();
        request.Customer.BirthDate = DateTime.UtcNow.AddYears(-71);

        var result = _service.Analyze(request);

        AssertRuleRejected(result, "Idade entre 18 e 70 anos");
    }

    [Fact]
    public void Analyze_WhenCustomerIsOver75AtContractEnd_ShouldRejectRule()
    {
        var request = CreateValidRequest();
        request.Customer.BirthDate = DateTime.UtcNow.AddYears(-70);
        request.TermMonths = 72;

        var result = _service.Analyze(request);

        AssertRuleRejected(result, "Idade ao final do contrato não ultrapassa 75 anos");
    }

    [Fact]
    public void Analyze_WhenRiskLevelIsHigh_ShouldRejectRiskRule()
    {
        var request = CreateValidRequest();
        request.CreditScore = 400;
        request.Customer.CreditScore = 400;
        request.EmploymentMonths = 2;
        request.RequestedAmount = 30000m;
        request.MonthlyIncome = 2000m;
        request.TermMonths = 12;

        var result = _service.Analyze(request);

        AssertRuleRejected(result, "Cliente não apresenta perfil de alto risco");
    }

    [Fact]
    public void Analyze_ShouldReturnAllTenRules()
    {
        var request = CreateValidRequest();

        var result = _service.Analyze(request);

        Assert.Equal(10, result.Rules.Count);
    }

    private static void AssertRuleRejected(CreditAnalysisResult result,
        string description)
    {
        var rule = Assert.Single(result.Rules.Where(x => x.Description == description));

        Assert.False(rule.Approved);
        Assert.Equal("Reprovado", result.Status);
    }
}