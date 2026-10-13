using Application.Common;
using Application.Dtos.Customers;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class CustomerService(
    ICustomerRepository repository,
    ICreditScoreProvider creditScoreProvider,
    ILogger<CustomerService> logger
) : ICustomerService
{
    private const int MaxPageSize = 100;

    public async Task<CustomerResponseDto> CreateCustomer(CreateCustomerDto dto, CancellationToken cancellationToken)
    {
        var email = InputNormalizer.Email(dto.Email);
        var cpf = InputNormalizer.Cpf(dto.Cpf);

        await EnsureUniqueAsync(email, cpf, currentId: null, cancellationToken);

        var creditScore = await creditScoreProvider.GetScoreAsync(cpf, cancellationToken);

        var customer = Customer.Create(
            InputNormalizer.Text(dto.Name),
            InputNormalizer.Text(dto.Phone),
            email,
            cpf,
            dto.BirthDate,
            creditScore);

        await repository.AddAsync(customer, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Cliente criado. CustomerId: {CustomerId}", customer.CustomerId);

        return customer.ToResponse();
    }

    public async Task<PagedResult<CustomerResponseDto>> GetAllCustomers(int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var (items, totalCount) = await repository.GetPagedAsync(page, pageSize, cancellationToken);

        return new PagedResult<CustomerResponseDto>(items.Select(c => c.ToResponse()).ToList(), page, pageSize, totalCount);
    }

    public async Task<CustomerResponseDto?> GetById(int id, CancellationToken cancellationToken)
    {
        var customer = await repository.GetByIdAsync(id, cancellationToken);

        return customer?.ToResponse();
    }

    public async Task<CustomerResponseDto?> UpdateCustomer(int id, UpdateCustomerDto dto, CancellationToken cancellationToken)
    {
        var customer = await repository.GetByIdAsync(id, cancellationToken);

        if (customer is null)
            return null;

        var email = InputNormalizer.Email(dto.Email);
        var cpf = InputNormalizer.Cpf(dto.Cpf);

        await EnsureUniqueAsync(email, cpf, currentId: id, cancellationToken);

        customer.Update(
            InputNormalizer.Text(dto.Name),
            InputNormalizer.Text(dto.Phone),
            email,
            cpf,
            dto.BirthDate);

        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Cliente atualizado. CustomerId: {CustomerId}", id);

        return customer.ToResponse();
    }

    public async Task<CustomerResponseDto?> SetActive(int id, bool active, CancellationToken cancellationToken)
    {
        var customer = await repository.GetByIdAsync(id, cancellationToken);

        if (customer is null)
            return null;

        if (customer.Active == active)
            return customer.ToResponse();

        customer.SetActive(active);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Status do cliente alterado. CustomerId: {CustomerId}, Active: {Active}", id, active);

        return customer.ToResponse();
    }

    private async Task EnsureUniqueAsync(string email, string cpf, int? currentId, CancellationToken cancellationToken)
    {
        var byEmail = await repository.GetByEmailAsync(email, cancellationToken);

        if (byEmail is not null && byEmail.CustomerId != currentId)
        {
            logger.LogWarning("Conflito: e-mail já pertence ao cliente {ConflictingCustomerId}.", byEmail.CustomerId);
            throw new InvalidOperationException("E-mail já cadastrado.");
        }

        var byCpf = await repository.GetByCpfAsync(cpf, cancellationToken);

        if (byCpf is not null && byCpf.CustomerId != currentId)
        {
            logger.LogWarning("Conflito: CPF já pertence ao cliente {ConflictingCustomerId}.", byCpf.CustomerId);
            throw new InvalidOperationException("CPF já cadastrado.");
        }
    }
}