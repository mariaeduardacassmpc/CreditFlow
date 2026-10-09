using Application.Dtos.Credit;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Events;
using Microsoft.Extensions.Logging;
using Moq;

namespace CreditFlow.Tests.Services;

public class CreditRequestServiceTests
{
    private readonly Mock<ICreditRequestRepository> _repository = new();
    private readonly Mock<ILogger<CreditRequestService>> _logger = new();
    private readonly Mock<ICreditScoreProvider> _creditScoreProvider = new();
    private readonly Mock<IKafkaProducer> _kafkaProducer = new();
    private readonly CreditRequestService _service;

    public CreditRequestServiceTests()
    {
        _service = new CreditRequestService(
            _repository.Object,
            _logger.Object,
            _creditScoreProvider.Object,
            _kafkaProducer.Object);
    }

    [Fact]
    public async Task CreateCreditRequest_ComDadosValidos_DeveCriarSolicitacao()
    {
        var customer = new Customer
        {
            CustomerId = 1,
            Name = "Maria",
            Email = "maria@teste.com",
            Cpf = "12345678900",
            Phone = "43999999999",
            Active = true,
            CreditScore = 750,
            BirthDate = new DateTime(1990, 1, 1)
        };

        var dto = new CreateCreditRequestDto
        {
            CustomerId = 1,
            RequestedAmount = 10000,
            MonthlyIncome = 5000,
            EmploymentMonths = 24,
            Purpose = CreditPurpose.Home,
            TermMonths = 12
        };

        CreditRequest? capturedRequest = null;

        _repository
            .Setup(x => x.GetCustomerAsync(1))
            .ReturnsAsync(customer);

        _repository
            .Setup(x => x.CreateAsync(It.IsAny<CreditRequest>()))
            .Callback<CreditRequest>(request => capturedRequest = request)
            .ReturnsAsync((CreditRequest request) =>
            {
                request.CreditRequestId = 10;
                return request;
            });

        _kafkaProducer
            .Setup(x => x.PublishAsync(
                "credit-request-created",
                It.IsAny<CreditRequestCreatedEvent>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateCreditRequest(dto);

        Assert.NotNull(capturedRequest);
        Assert.Equal(10, result.CreditRequestId);
        Assert.Equal(1, result.CustomerId);
        Assert.Equal(10000, result.RequestedAmount);
        Assert.Equal(5000, result.MonthlyIncome);
        Assert.Equal(750, result.CreditScore);
        Assert.Equal("Pending", result.Status);
        Assert.Equal(CreditPurpose.Home, result.Purpose);
        Assert.Equal(12, result.TermMonths);
        Assert.Equal(24, result.EmploymentMonths);
        Assert.Same(customer, result.Customer);

        _repository.Verify(x => x.CreateAsync(It.IsAny<CreditRequest>()), Times.Once);
        _kafkaProducer.Verify(
            x => x.PublishAsync(
                "credit-request-created",
                It.Is<CreditRequestCreatedEvent>(e => e.CreditRequestId == 10)),
            Times.Once);
    }

    [Fact]
    public async Task CreateCreditRequest_ClienteInexistente_DeveLancarExcecao()
    {
        var dto = new CreateCreditRequestDto
        {
            CustomerId = 99,
            RequestedAmount = 10000,
            MonthlyIncome = 5000,
            EmploymentMonths = 12,
            Purpose = CreditPurpose.Personal,
            TermMonths = 12
        };

        _repository
            .Setup(x => x.GetCustomerAsync(99))
            .ReturnsAsync((Customer?)null);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateCreditRequest(dto));

        Assert.Equal("Cliente não encontrado.", exception.Message);

        _repository.Verify(
            x => x.CreateAsync(It.IsAny<CreditRequest>()),
            Times.Never);

        _kafkaProducer.Verify(
            x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<CreditRequestCreatedEvent>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateCreditRequest_DevePublicarEventoAposCriacao()
    {
        var customer = new Customer
        {
            CustomerId = 2,
            Name = "Ana",
            Email = "ana@teste.com",
            Cpf = "98765432100",
            Phone = "43988888888",
            Active = true,
            CreditScore = 680,
            BirthDate = new DateTime(1990, 1, 1)
        };

        var dto = new CreateCreditRequestDto
        {
            CustomerId = 2,
            RequestedAmount = 5000,
            MonthlyIncome = 3000,
            EmploymentMonths = 18,
            Purpose = CreditPurpose.DebtConsolidation,
            TermMonths = 6
        };

        _repository
            .Setup(x => x.GetCustomerAsync(2))
            .ReturnsAsync(customer);

        _repository
            .Setup(x => x.CreateAsync(It.IsAny<CreditRequest>()))
            .ReturnsAsync((CreditRequest request) =>
            {
                request.CreditRequestId = 20;
                return request;
            });

        _kafkaProducer
            .Setup(x => x.PublishAsync(
                "credit-request-created",
                It.IsAny<CreditRequestCreatedEvent>()))
            .Returns(Task.CompletedTask);

        await _service.CreateCreditRequest(dto);

        _kafkaProducer.Verify(
            x => x.PublishAsync(
                "credit-request-created",
                It.Is<CreditRequestCreatedEvent>(e => e.CreditRequestId == 20)),
            Times.Once);
    }

    [Fact]
    public async Task GetAllCreditRequests_DeveRetornarSolicitacoesMapeadas()
    {
        var customer = new Customer
        {
            CustomerId = 1,
            Name = "Maria",
            Email = "maria@teste.com",
            Cpf = "12345678900",
            Phone = "43999999999",
            Active = true,
            CreditScore = 750,
            BirthDate = new DateTime(1990, 1, 1)
        };

        var requests = new List<CreditRequest>
        {
            new()
            {
                CreditRequestId = 1,
                CustomerId = 1,
                Customer = customer,
                RequestedAmount = 10000,
                MonthlyIncome = 5000,
                CreditScore = 750,
                EmploymentMonths = 24,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                Purpose = CreditPurpose.Business,
                TermMonths = 12
            },
            new()
            {
                CreditRequestId = 2,
                CustomerId = 1,
                Customer = customer,
                RequestedAmount = 5000,
                MonthlyIncome = 5000,
                CreditScore = 750,
                EmploymentMonths = 24,
                Status = "Approved",
                CreatedAt = DateTime.UtcNow,
                Purpose = CreditPurpose.Personal,
                TermMonths = 6
            }
        };

        _repository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(requests);

        var result = (await _service.GetAllCreditRequests()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].CreditRequestId);
        Assert.Equal(10000, result[0].RequestedAmount);
        Assert.Equal("Pending", result[0].Status);
        Assert.Equal(2, result[1].CreditRequestId);
        Assert.Equal("Approved", result[1].Status);
    }

    [Fact]
    public async Task GetAllCreditRequests_SemSolicitacoes_DeveRetornarListaVazia()
    {
        _repository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<CreditRequest>());

        var result = await _service.GetAllCreditRequests();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetById_SolicitacaoExistente_DeveRetornarDetalhes()
    {
        var customer = new Customer
        {
            CustomerId = 1,
            Name = "Maria",
            Email = "maria@teste.com",
            Cpf = "12345678900",
            Phone = "43999999999",
            Active = true,
            CreditScore = 750,
            BirthDate = new DateTime(1990, 1, 1)
        };

        var request = new CreditRequest
        {
            CreditRequestId = 10,
            CustomerId = 1,
            Customer = customer,
            RequestedAmount = 10000,
            MonthlyIncome = 5000,
            CreditScore = 750,
            EmploymentMonths = 24,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            Purpose = CreditPurpose.Vehicle,
            TermMonths = 12
        };

        _repository
            .Setup(x => x.GetByIdAsync(10))
            .ReturnsAsync(request);

        var result = await _service.GetById(10);

        Assert.NotNull(result);
        Assert.Equal(10, result.CreditRequestId);
        Assert.Equal(1, result.CustomerId);
        Assert.Equal("Maria", result.CustomerName);
        Assert.Equal("maria@teste.com", result.CustomerEmail);
        Assert.Equal(10000, result.RequestedAmount);
        Assert.Equal(5000, result.MonthlyIncome);
        Assert.Equal(750, result.CreditScore);
        Assert.Equal(24, result.EmploymentMonths);
        Assert.Equal("Pending", result.Status);
    }

    [Fact]
    public async Task GetById_SolicitacaoInexistente_DeveRetornarNull()
    {
        _repository
            .Setup(x => x.GetByIdAsync(99))
            .ReturnsAsync((CreditRequest?)null);

        var result = await _service.GetById(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateCreditRequest_DeveUsarScoreDoCliente()
    {
        var customer = new Customer
        {
            CustomerId = 3,
            Name = "João",
            Email = "joao@teste.com",
            Cpf = "11122233344",
            Phone = "43977777777",
            Active = true,
            CreditScore = 620,
            BirthDate = new DateTime(1990, 1, 1)
        };

        var dto = new CreateCreditRequestDto
        {
            CustomerId = 3,
            RequestedAmount = 2000,
            MonthlyIncome = 2500,
            EmploymentMonths = 6,
            Purpose = CreditPurpose.Business,
            TermMonths = 6
        };

        _repository
            .Setup(x => x.GetCustomerAsync(3))
            .ReturnsAsync(customer);

        _repository
            .Setup(x => x.CreateAsync(It.IsAny<CreditRequest>()))
            .ReturnsAsync((CreditRequest request) => request);

        _kafkaProducer
            .Setup(x => x.PublishAsync(
                "credit-request-created",
                It.IsAny<CreditRequestCreatedEvent>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateCreditRequest(dto);

        Assert.Equal(customer.CreditScore, result.CreditScore);

        _creditScoreProvider.Verify(
            x => x.GetScoreAsync(It.IsAny<string>()),
            Times.Never);
    }
}