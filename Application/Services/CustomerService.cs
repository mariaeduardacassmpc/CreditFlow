using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class CustomerService(ICustomerRepository repository, ILogger<CustomerService> logger, ICreditScoreProvider creditScoreProvider)
{
    public async Task<Customer> CreateCustomer(Customer customer)
    {
        logger.LogInformation("Criando cliente.");

        var creditScore = await creditScoreProvider.GetScoreAsync(customer.Cpf);

        logger.LogInformation("Score retornado para o CPF {Cpf}: {CreditScore}", customer.Cpf, creditScore);

        customer.CreditScore = creditScore;

        var existingCustomer = await repository.GetByEmailAsync(customer.Email);

        if (existingCustomer is not null)
        {
            logger.LogWarning("Tentativa de criar cliente duplicado. Email: {Email}", customer.Email);

            throw new InvalidOperationException("E-mail já existe.");
        }

        await repository.AddAsync(customer);

        await repository.SaveChangesAsync();

        logger.LogInformation("Cliente criado com sucesso. Id: {CustomerId}", customer.CustomerId);

        return customer;
    }

    public async Task<IEnumerable<Customer>> GetAllCustomers()
    {
        logger.LogInformation("Buscando todos os clientes.");

        return await repository.GetAllAsync();
    }

    public async Task<Customer?> GetById(int id)
    {
        logger.LogInformation("Buscando cliente por Id: {CustomerId}", id);

        var customer = await repository.GetByIdAsync(id);

        if (customer is null)
        {
            logger.LogWarning("Cliente não encontrado. Id: {CustomerId}", id);

            throw new InvalidOperationException($"Cliente com Id {id} não encontrado.");
        }

        return customer;
    }

    public async Task<Customer?> UpdateCustomer(int id, Customer customer)
    {
        logger.LogInformation("Atualizando cliente. Id: {CustomerId}", id);

        var existingCustomer = await repository.GetByIdAsync(id);

        if (existingCustomer is null)
        {
            logger.LogWarning("Cliente não encontrado para atualização. Id: {CustomerId}", id);

            throw new InvalidOperationException($"Cliente com Id {id} não encontrado.");
        }

        var emailExists = await repository.GetByEmailAsync(customer.Email);

        if (emailExists is not null &&
            emailExists.CustomerId != id)
        {
            logger.LogWarning("Tentativa de atualizar cliente com e-mail já cadastrado. Email: {Email}", customer.Email);

            throw new InvalidOperationException("E-mail já existe.");
        }

        existingCustomer.Name = customer.Name;
        existingCustomer.Phone = customer.Phone;
        existingCustomer.Email = customer.Email;
        existingCustomer.Active = customer.Active;
        existingCustomer.Cpf = customer.Cpf;
        existingCustomer.BirthDate = customer.BirthDate;

        await repository.SaveChangesAsync();

        logger.LogInformation("Cliente atualizado com sucesso. Id: {CustomerId}", id);

        return existingCustomer;
    }

    public async Task<Customer> ToggleActive(int id)
    {
        logger.LogInformation("Alterando status do cliente. Id: {CustomerId}", id);

        var customer = await repository.GetByIdAsync(id);

        if (customer is null)
        {
            logger.LogWarning("Cliente não encontrado para alteração de status. Id: {CustomerId}", id);
            throw new InvalidOperationException($"Cliente com Id {id} não encontrado.");
        }

        customer.Active = !customer.Active;

        await repository.SaveChangesAsync();

        logger.LogInformation("Status do cliente alterado. Id: {CustomerId}, Ativo: {Active}", id, customer.Active);

        return customer;
    }
}