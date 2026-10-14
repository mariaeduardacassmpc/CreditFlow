using Application.Common;
using Application.Dtos.Customers;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace CreditFlow.Tests.Services;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _repository = new();
    private readonly Mock<ICreditScoreProvider> _creditScoreProvider = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly CustomerService _service;
    private readonly Mock<ILogger<CustomerService>> _logger = new();

    public CustomerServiceTests()
    {
        _service = new CustomerService(
             _repository.Object,
             _creditScoreProvider.Object,
             _logger.Object);
    }

    private static void DefinirCustomerId(Customer customer, int id)
    {
        typeof(Customer)
            .GetProperty(nameof(Customer.CustomerId))!
            .SetValue(customer, id);
    }

    private static Customer CriarCliente(
         string name = "Maria",
         string email = "maria@teste.com",
         string cpf = "12345678900",
         string phone = "43999999999")
    {
        return Customer.Create(
            name,
            phone,
            email,
            cpf,
            new DateOnly(1990, 1, 1),
            750);
    }

    private static CreateCustomerDto CriarDto()
    {
        return new CreateCustomerDto
        {
            Name = "Maria",
            Email = "maria@teste.com",
            Cpf = "12345678900",
            Phone = "43999999999",
            BirthDate = new DateOnly(1990, 1, 1)
        };
    }

    private static UpdateCustomerDto CriarUpdateDto(string nome = "Maria", string email = "maria@teste.com")
    {
        return new UpdateCustomerDto
        {
            Name = nome,
            Email = email,
            Cpf = "12345678900",
            Phone = "43999999999",
            BirthDate = new DateOnly(1990, 1, 1)
        };
    }

    [Fact]
    public async Task CreateCustomer_ComDadosValidos_DeveCriarCliente()
    {
        var dto = CriarDto();
        var customer = CriarCliente();

        _mapper
            .Setup(m => m.Map<Customer>(dto))
            .Returns(customer);

        _creditScoreProvider
           .Setup(x => x.GetScoreAsync(
               dto.Cpf,
               It.IsAny<CancellationToken>()))
           .ReturnsAsync(750);

        _repository
            .Setup(x => x.GetByEmailAsync(
                dto.Email.Trim().ToLowerInvariant(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        _repository
            .Setup(x => x.AddAsync(
                It.IsAny<Customer>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repository
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateCustomer(
            dto,
            CancellationToken.None);

        Assert.Equal(750, result.CreditScore);
        Assert.Equal("maria@teste.com", result.Email);

        _repository.Verify(
            x => x.AddAsync(
                It.IsAny<Customer>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repository.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateCustomer_EmailExistente_DeveLancarExcecao()
    {
        var dto = CriarDto();

        _creditScoreProvider
            .Setup(x => x.GetScoreAsync(
                dto.Cpf,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(750);

        _repository
            .Setup(x => x.GetByEmailAsync(
                dto.Email.Trim().ToLowerInvariant(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CriarCliente());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateCustomer(
                dto,
                CancellationToken.None));

        _repository.Verify(
            x => x.AddAsync(
                It.IsAny<Customer>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repository.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAllCustomers_DeveRetornarTodosOsClientes()
    {
        var customers = new List<Customer>
        {
            CriarCliente(),
            CriarCliente(
                "Ana",
                "ana@teste.com",
                "98765432100",
                "43988888888")
        };

        _repository
            .Setup(x => x.GetPagedAsync(
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((customers, customers.Count));

        var result = await _service.GetAllCustomers(
            page: 1,
            pageSize: 10,
            cancellationToken: CancellationToken.None);

        Assert.Equal(2, result.Items.Count());

        Assert.Contains(
            result.Items,
            x => x.Email == "maria@teste.com");

        Assert.Contains(
            result.Items,
            x => x.Email == "ana@teste.com");
    }

    [Fact]
    public async Task GetById_ClienteExistente_DeveRetornarCliente()
    {
        var customer = CriarCliente();

        _repository
            .Setup(x => x.GetByIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var result = await _service.GetById(
            1,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Maria", result!.Name);
    }

    [Fact]
    public async Task GetById_ClienteInexistente_DeveRetornarNull()
    {
        _repository
            .Setup(x => x.GetByIdAsync(
                99,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var result = await _service.GetById(
            99,
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateCustomer_ComDadosValidos_DeveAtualizarCliente()
        {
        var existingCustomer = CriarCliente();
        DefinirCustomerId(existingCustomer, 1);

        var dto = CriarUpdateDto("Maria Nova");

        _repository
            .Setup(x => x.GetByIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        _repository
            .Setup(x => x.GetByEmailAsync(
                InputNormalizer.Email(dto.Email),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        _repository
            .Setup(x => x.GetByCpfAsync(
                InputNormalizer.Cpf(dto.Cpf),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        _repository
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateCustomer(
            1,
            dto,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Maria Nova", result.Name);
        Assert.Equal(
            InputNormalizer.Email(dto.Email),
            result.Email);

        _repository.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateCustomer_ClienteInexistente_DeveRetornarNull()
    {
        var dto = CriarUpdateDto();

        _repository
            .Setup(x => x.GetByIdAsync(
                99,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var result = await _service.UpdateCustomer(
            99,
            dto,
            CancellationToken.None);

        Assert.Null(result);

        _repository.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateCustomer_EmailDeOutroCliente_DeveLancarExcecao()
    {
        var existingCustomer = CriarCliente();
        var anotherCustomer = CriarCliente(
            "Ana",
            "ana@teste.com",
            "98765432100",
            "43988888888");

        var dto = CriarUpdateDto(
            "Maria",
            "ana@teste.com");

        _repository
            .Setup(x => x.GetByIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        _repository
            .Setup(x => x.GetByEmailAsync(
                dto.Email.Trim().ToLowerInvariant(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(anotherCustomer);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateCustomer(
                1,
                dto,
                CancellationToken.None));

        Assert.Equal("E-mail já cadastrado.", exception.Message);

        _repository.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ToggleActive_ClienteAtivo_DeveDesativar()
    {
        var customer = CriarCliente();

        _repository
            .Setup(x => x.GetByIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _repository
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.SetActive(
            1, false,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result!.Active);

        _repository.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ToggleActive_ClienteInativo_DeveAtivar()
    {
        var customer = CriarCliente();

        customer.SetActive(false);

        _repository
            .Setup(x => x.GetByIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _repository
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.SetActive(
            1, true, 
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result!.Active);

        _repository.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ToggleActive_ClienteInexistente_DeveRetornarNull()
    {
        _repository
            .Setup(x => x.GetByIdAsync(
                99,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var result = await _service.SetActive(
            99, true, 
            CancellationToken.None);

        Assert.Null(result);

        _repository.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
