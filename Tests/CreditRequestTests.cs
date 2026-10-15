using Application.Common;
using Application.Dtos.Credit;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace CreditFlow.Tests.Services;

public class CreditRequestServiceTests
{
    private readonly Mock<ICreditRequestRepository> _repository = new();
    private readonly Mock<ILogger<CreditRequestService>> _logger = new();
    private readonly CreditRequestService _service;

    public CreditRequestServiceTests()
    {
        _service = new CreditRequestService(
            _repository.Object,
            _logger.Object);
    }

    [Fact]
    public async Task CreateCreditRequest_ComDadosValidos_DeveCriarSolicitacao()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        var customer = CreateCustomer(
            "Maria",
            "maria@teste.com",
            "12345678900",
            750);

        var dto = CreateDto(
            1,
            10000m,
            5000m,
            24,
            CreditPurpose.Home,
            12);

        CreditRequest? capturedRequest = null;

        _repository
            .Setup(x => x.GetCustomerAsync(1, token))
            .ReturnsAsync(customer);

        _repository
            .Setup(x => x.CreateAsync(
                It.IsAny<CreditRequest>(),
                token))
            .Callback<CreditRequest, CancellationToken>(
                (request, _) => capturedRequest = request)
            .ReturnsAsync((CreditRequest request, CancellationToken _) =>
            {
                request.CreditRequestId = 10;
                return request;
            });

        var result = await _service.CreateCreditRequest(dto, token);

        Assert.NotNull(capturedRequest);
        Assert.Equal(10, result.CreditRequestId);
        Assert.Equal(customer.CustomerId, result.CustomerId);
        Assert.Equal(10000m, result.RequestedAmount);
        Assert.Equal(5000m, result.MonthlyIncome);
        Assert.Equal(750, result.CreditScore);
        Assert.Equal(nameof(Status.Pending), result.Status);
        Assert.Equal(nameof(CreditPurpose.Home), result.Purpose);
        Assert.Equal(12, result.TermMonths);
        Assert.Equal(24, result.EmploymentMonths);

        Assert.Equal(customer.CustomerId, capturedRequest.CustomerId);
        Assert.Equal(customer, capturedRequest.Customer);

        _repository.Verify(
            x => x.GetCustomerAsync(1, token),
            Times.Once);

        _repository.Verify(
            x => x.CreateAsync(
                It.IsAny<CreditRequest>(),
                token),
            Times.Once);
    }

    [Fact]
    public async Task CreateCreditRequest_ClienteInexistente_DeveLancarExcecao()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        var dto = CreateDto(
            99,
            10000m,
            5000m,
            12,
            CreditPurpose.Personal,
            12);

        _repository
            .Setup(x => x.GetCustomerAsync(99, token))
            .ReturnsAsync((Customer?)null);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateCreditRequest(dto, token));

        Assert.Equal("Cliente não encontrado.", exception.Message);

        _repository.Verify(
            x => x.CreateAsync(
                It.IsAny<CreditRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateCreditRequest_ClienteInativo_DeveLancarExcecao()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        var customer = CreateCustomer(
            "Ana",
            "ana@teste.com",
            "98765432100",
            680);

        customer.SetActive(false);

        var dto = CreateDto(
            2,
            5000m,
            3000m,
            18,
            CreditPurpose.Personal,
            6);

        _repository
            .Setup(x => x.GetCustomerAsync(2, token))
            .ReturnsAsync(customer);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateCreditRequest(dto, token));

        Assert.Equal("O cliente está inativo.", exception.Message);

        _repository.Verify(
            x => x.CreateAsync(
                It.IsAny<CreditRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateCreditRequest_DeveUsarScoreDoCliente()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        var customer = CreateCustomer(
            "João",
            "joao@teste.com",
            "11122233344",
            620);

        var dto = CreateDto(
            3,
            2000m,
            2500m,
            6,
            CreditPurpose.Business,
            6);

        _repository
            .Setup(x => x.GetCustomerAsync(3, token))
            .ReturnsAsync(customer);

        _repository
            .Setup(x => x.CreateAsync(
                It.IsAny<CreditRequest>(),
                token))
            .ReturnsAsync((CreditRequest request, CancellationToken _) =>
                request);

        var result = await _service.CreateCreditRequest(dto, token);

        Assert.Equal(customer.CreditScore, result.CreditScore);
    }

    [Fact]
    public async Task GetAllCreditRequests_DeveRetornarSolicitacoesMapeadas()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        var customer = CreateCustomer(
            "Maria",
            "maria@teste.com",
            "12345678900",
            750);

        var requests = new List<CreditRequest>
        {
            new()
            {
                CreditRequestId = 1,
                CustomerId = 1,
                Customer = customer,
                RequestedAmount = 10000m,
                MonthlyIncome = 5000m,
                CreditScore = 750,
                EmploymentMonths = 24,
                Status = Status.Pending,
                CreatedAt = DateTime.UtcNow,
                Purpose = CreditPurpose.Business,
                TermMonths = 12
            },
            new()
            {
                CreditRequestId = 2,
                CustomerId = 1,
                Customer = customer,
                RequestedAmount = 5000m,
                MonthlyIncome = 5000m,
                CreditScore = 750,
                EmploymentMonths = 24,
                Status = Status.Approved,
                CreatedAt = DateTime.UtcNow,
                Purpose = CreditPurpose.Personal,
                TermMonths = 6
            }
        };

        var pagedRequests = new PagedResult<CreditRequest>(
            requests,
            1,
            10,
            requests.Count);

        _repository
            .Setup(x => x.GetAllAsync(1, 10, null, null, token))
            .ReturnsAsync(pagedRequests);

        var result = await _service.GetAllCreditRequests(
            page: 1,
            pageSize: 10,
            customerId: null,
            status: null,
            cancellationToken: token);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.Items[0].CreditRequestId);
        Assert.Equal(10000m, result.Items[0].RequestedAmount);
        Assert.Equal(Status.Pending, result.Items[0].Status);
        Assert.Equal(2, result.Items[1].CreditRequestId);
        Assert.Equal(Status.Approved, result.Items[1].Status);
        Assert.Equal(2, result.TotalCount);

        _repository.Verify(
            x => x.GetAllAsync(1, 10, null, null, token),
            Times.Once);
    }

    [Fact]
    public async Task GetAllCreditRequests_SemSolicitacoes_DeveRetornarListaVazia()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        var emptyPagedRequests = new PagedResult<CreditRequest>(
            new List<CreditRequest>(),
            1,
            10,
            0);

        _repository
            .Setup(x => x.GetAllAsync(1, 10, null, null, token))
            .ReturnsAsync(emptyPagedRequests);

        var result = await _service.GetAllCreditRequests(
            page: 1,
            pageSize: 10,
            customerId: null,
            status: null,
            cancellationToken: token);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);

        _repository.Verify(
            x => x.GetAllAsync(1, 10, null, null, token),
            Times.Once);
    }

    [Fact]
    public async Task GetById_SolicitacaoExistente_DeveRetornarDetalhes()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        var customer = CreateCustomer(
            "Maria",
            "maria@teste.com",
            "12345678900",
            750);

        var request = new CreditRequest
        {
            CreditRequestId = 10,
            CustomerId = 1,
            Customer = customer,
            RequestedAmount = 10000m,
            MonthlyIncome = 5000m,
            CreditScore = 750,
            EmploymentMonths = 24,
            Status = Status.Pending,
            CreatedAt = DateTime.UtcNow,
            Purpose = CreditPurpose.Vehicle,
            TermMonths = 12
        };

        _repository
            .Setup(x => x.GetByIdAsync(10, token))
            .ReturnsAsync(request);

        var result = await _service.GetById(10, token);

        Assert.NotNull(result);
        Assert.Equal(10, result.CreditRequestId);
        Assert.Equal(1, result.CustomerId);
        Assert.Equal("Maria", result.CustomerName);
        Assert.Equal("maria@teste.com", result.CustomerEmail);
        Assert.Equal(10000m, result.RequestedAmount);
        Assert.Equal(5000m, result.MonthlyIncome);
        Assert.Equal(750, result.CreditScore);
        Assert.Equal(24, result.EmploymentMonths);
        Assert.Equal(Status.Pending, result.Status);

        _repository.Verify(
            x => x.GetByIdAsync(10, token),
            Times.Once);
    }

    [Fact]
    public async Task GetById_SolicitacaoInexistente_DeveRetornarNull()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _repository
            .Setup(x => x.GetByIdAsync(99, token))
            .ReturnsAsync((CreditRequest?)null);

        var result = await _service.GetById(99, token);

        Assert.Null(result);

        _repository.Verify(
            x => x.GetByIdAsync(99, token),
            Times.Once);
    }

    private static Customer CreateCustomer(
        string name,
        string email,
        string cpf,
        int creditScore)
    {
        return Customer.Create(
            name,
            "43999999999",
            email,
            cpf,
            new DateOnly(1990, 1, 1),
            creditScore);
    }

    private static CreateCreditRequestDto CreateDto(
        int customerId,
        decimal requestedAmount,
        decimal monthlyIncome,
        int employmentMonths,
        CreditPurpose purpose,
        int termMonths)
    {
        return new CreateCreditRequestDto
        {
            CustomerId = customerId,
            RequestedAmount = requestedAmount,
            MonthlyIncome = monthlyIncome,
            EmploymentMonths = employmentMonths,
            Purpose = purpose,
            TermMonths = termMonths
        };
    }
}