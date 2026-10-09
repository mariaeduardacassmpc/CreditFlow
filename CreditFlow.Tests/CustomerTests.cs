
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CreditFlow.Tests.Services;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _repository = new();
    private readonly Mock<ILogger<CustomerService>> _logger = new();
    private readonly Mock<ICreditScoreProvider> _creditScoreProvider = new();
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        _service = new CustomerService(
            _repository.Object,
            _logger.Object,
            _creditScoreProvider.Object);
    }

    [Fact]
    public async Task CreateCustomer_ComDadosValidos_DeveCriarCliente()
    {
        var customer = new Customer
        {
            Name = "Maria",
            Email = "maria@teste.com",
            Cpf = "12345678900",
            Phone = "43999999999",
            Active = true,
            BirthDate = new DateTime(1990, 1, 1),
        };

        _creditScoreProvider
            .Setup(x => x.GetScoreAsync(customer.Cpf))
            .ReturnsAsync(750);

        _repository
            .Setup(x => x.GetByEmailAsync(customer.Email))
            .ReturnsAsync((Customer?)null);

        var result = await _service.CreateCustomer(customer);

        Assert.Equal(750, result.CreditScore);
        Assert.Same(customer, result);

        _repository.Verify(
            x => x.AddAsync(customer),
            Times.Once);

        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task CreateCustomer_EmailExistente_DeveLancarExcecao()
    {
        var customer = new Customer
        {
            Name = "Maria",
            Email = "maria@teste.com",
            Cpf = "12345678900",
            Phone = "43999999999",
            BirthDate = new DateTime(1990, 1, 1),
            Active = true
        };

        _creditScoreProvider
            .Setup(x => x.GetScoreAsync(customer.Cpf))
            .ReturnsAsync(750);

        _repository
            .Setup(x => x.GetByEmailAsync(customer.Email))
            .ReturnsAsync(new Customer
            {
                Name = "Outra pessoa",
                Email = customer.Email,
                Cpf = "98765432100",
                Phone = "43988888888",
                BirthDate = new DateTime(1990, 1, 1),
                Active = true,
            });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateCustomer(customer));

        Assert.Equal("E-mail já existe.", exception.Message);

        _repository.Verify(
            x => x.AddAsync(It.IsAny<Customer>()),
            Times.Never);

        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task GetAllCustomers_DeveRetornarTodosOsClientes()
    {
        var customers = new List<Customer>
        {
            new()
            {
                Name = "Maria",
                Email = "maria@teste.com",
                Cpf = "12345678900",
                Phone = "43999999999",
                Active = true,
                BirthDate = new DateTime(1990, 1, 1),
            },
            new()
            {
                Name = "Ana",
                Email = "ana@teste.com",
                Cpf = "98765432100",
                Phone = "43988888888",
                Active = true,
                BirthDate = new DateTime(1990, 1, 1),
            }
        };

        _repository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(customers);

        var result = await _service.GetAllCustomers();

        Assert.Equal(2, result.Count());
        Assert.Contains(result, x => x.Email == "maria@teste.com");
        Assert.Contains(result, x => x.Email == "ana@teste.com");
    }

    [Fact]
    public async Task GetById_ClienteExistente_DeveRetornarCliente()
    {
        var customer = new Customer
        {
            CustomerId = 1,
            Name = "Maria",
            Email = "maria@teste.com",
            Cpf = "12345678900",
            Phone = "43999999999",
            Active = true,
            BirthDate = new DateTime(1990, 1, 1),
        };

        _repository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(customer);

        var result = await _service.GetById(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.CustomerId);
        Assert.Equal("Maria", result.Name);
    }

    [Fact]
    public async Task GetById_ClienteInexistente_DeveLancarExcecao()
    {
        _repository
            .Setup(x => x.GetByIdAsync(99))
            .ReturnsAsync((Customer?)null);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetById(99));

        Assert.Equal("Cliente com Id 99 não encontrado.", exception.Message);
    }

    [Fact]
    public async Task UpdateCustomer_ComDadosValidos_DeveAtualizarCliente()
    {
        var existingCustomer = new Customer
        {
            CustomerId = 1,
            Name = "Maria Antiga",
            Email = "maria@teste.com",
            Cpf = "12345678900",
            Phone = "43911111111",
            Active = true,
            BirthDate = new DateTime(1990, 1, 1),
        };

        var updatedCustomer = new Customer
        {
            Name = "Maria Nova",
            Email = "maria@teste.com",
            Cpf = "12345678900",
            Phone = "43999999999",
            Active = false,
            BirthDate = new DateTime(1990, 1, 1),
        };

        _repository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(existingCustomer);

        _repository
            .Setup(x => x.GetByEmailAsync(updatedCustomer.Email))
            .ReturnsAsync(existingCustomer);

        var result = await _service.UpdateCustomer(1, updatedCustomer);

        Assert.NotNull(result);
        Assert.Equal("Maria Nova", result.Name);
        Assert.Equal("43999999999", result.Phone);
        Assert.False(result.Active);

        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task UpdateCustomer_ClienteInexistente_DeveLancarExcecao()
    {
        var customer = new Customer
        {
            Name = "Maria",
            Email = "maria@teste.com",
            Cpf = "12345678900",
            Phone = "43999999999",
            Active = true,
            BirthDate = new DateTime(1990, 1, 1),
        };

        _repository
            .Setup(x => x.GetByIdAsync(99))
            .ReturnsAsync((Customer?)null);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateCustomer(99, customer));

        Assert.Equal("Cliente com Id 99 não encontrado.", exception.Message);

        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task UpdateCustomer_EmailDeOutroCliente_DeveLancarExcecao()
    {
        var existingCustomer = new Customer
        {
            CustomerId = 1,
            Name = "Maria",
            Email = "maria@teste.com",
            Cpf = "12345678900",
            Phone = "43999999999",
            Active = true,
            BirthDate = new DateTime(1990, 1, 1),
        };

        var anotherCustomer = new Customer
        {
            CustomerId = 2,
            Name = "Ana",
            Email = "ana@teste.com",
            Cpf = "98765432100",
            Phone = "43988888888",
            Active = true,
            BirthDate = new DateTime(1990, 1, 1),
        };

        var updatedCustomer = new Customer
        {
            Name = "Maria",
            Email = "ana@teste.com",
            Cpf = "12345678900",
            Phone = "43999999999",
            Active = true,
            BirthDate = new DateTime(1990, 1, 1),
        };

        _repository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(existingCustomer);

        _repository
            .Setup(x => x.GetByEmailAsync(updatedCustomer.Email))
            .ReturnsAsync(anotherCustomer);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateCustomer(1, updatedCustomer));

        Assert.Equal("E-mail já existe.", exception.Message);

        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task ToggleActive_ClienteAtivo_DeveDesativar()
    {
        var customer = new Customer
        {
            CustomerId = 1,
            Name = "Maria",
            Email = "maria@teste.com",
            Cpf = "12345678900",
            Phone = "43999999999",
            Active = true,
            BirthDate = new DateTime(1990, 1, 1),
        };

        _repository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(customer);

        var result = await _service.ToggleActive(1);

        Assert.False(result.Active);

        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task ToggleActive_ClienteInativo_DeveAtivar()
    {
        var customer = new Customer
        {
            CustomerId = 1,
            Name = "Maria",
            Email = "maria@teste.com",
            Cpf = "12345678900",
            Phone = "43999999999",
            Active = false,
            BirthDate = new DateTime(1990, 1, 1),
        };

        _repository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(customer);

        var result = await _service.ToggleActive(1);

        Assert.True(result.Active);

        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task ToggleActive_ClienteInexistente_DeveLancarExcecao()
    {
        _repository
            .Setup(x => x.GetByIdAsync(99))
            .ReturnsAsync((Customer?)null);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ToggleActive(99));

        Assert.Equal("Cliente com Id 99 não encontrado.", exception.Message);

        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }
}