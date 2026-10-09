using Application.Dtos.Customers;
using Application.Interfaces;
using Domain.Entities;
using Application.Exceptions;

namespace Application.Services;

public class CustomerService(ICustomerRepository repository, ICreditScoreProvider creditScoreProvider, IMapper mapper) : ICustomerService
{
    public async Task<Customer> CreateCustomer(CreateCustomerDto dto, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var email = dto.Email.Trim().ToLowerInvariant();
        var existingCustomer = await repository.GetByEmailAsync(email, cancellationToken);

        if (existingCustomer is not null)
            throw new BusinessConflictException("E-mail já cadastrado.");

        var customer = mapper.Map<Customer>(dto);
        customer.CreditScore = await creditScoreProvider.GetScoreAsync(customer.Cpf);
        cancellationToken.ThrowIfCancellationRequested();

        customer.Active = true;

        await repository.AddAsync(customer, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return customer;
    }

    public async Task<IEnumerable<Customer>> GetAllCustomers(CancellationToken cancellationToken)
    {
        return await repository.GetAllAsync(cancellationToken);
    }

    public async Task<Customer?> GetById(int id, CancellationToken cancellationToken)
    {
        return await repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Customer?> UpdateCustomer(int id, UpdateCustomerDto dto, CancellationToken cancellationToken)
    {
        var existingCustomer = await repository.GetByIdAsync(id, cancellationToken);

        if (existingCustomer is null)
            return null;

        var email = dto.Email.Trim().ToLowerInvariant();
        var customerWithEmail = await repository.GetByEmailAsync(email, cancellationToken);

        if (customerWithEmail is not null && customerWithEmail.CustomerId != id)
            throw new BusinessConflictException("E-mail já cadastrado.");

        var updatedData = mapper.Map<Customer>(dto);

        existingCustomer.Name = updatedData.Name;
        existingCustomer.Phone = updatedData.Phone;
        existingCustomer.Email = email;
        existingCustomer.Cpf = updatedData.Cpf;
        existingCustomer.BirthDate = updatedData.BirthDate;

        await repository.SaveChangesAsync(cancellationToken);

        return existingCustomer;
    }

    public async Task<Customer?> ToggleActive(int id, CancellationToken cancellationToken)
    {
        var customer = await repository.GetByIdAsync(id, cancellationToken);

        if (customer is null)
            return null;

        customer.Active = !customer.Active;

        await repository.SaveChangesAsync(cancellationToken);

        return customer;
    }
}