using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.ExternalServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class CustomerService(CreditFlowDbContext context, ILogger<CustomerService> logger, ICreditScoreProvider creditScoreProvider)
{
    public async Task<Customer> CreateCustomer(Customer customer)
    {
        logger.LogInformation("Criando cliente.");

        var creditScore = await creditScoreProvider
            .GetScoreAsync(customer.Cpf);

        logger.LogInformation(
    "Score retornado para o CPF {Cpf}: {CreditScore}",
    customer.Cpf,
    creditScore);

        customer.CreditScore = creditScore;
        context.Customers.Add(customer);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Customer_Email") == true)
        {
            logger.LogWarning("Tentativa de criar cliente duplicado. Email: {Email}", customer.Email);
            throw new InvalidOperationException("E-mail já existe.");
        }

        logger.LogInformation("Cliente criado com sucesso. Id: {CustomerId}", customer.CustomerId);

        return customer;
    }

    public async Task<IEnumerable<Customer>> GetAllCustomers()
    {
        logger.LogInformation("Buscando todos os clientes");

        return await context.Customers.ToListAsync();
    }

    public async Task<Customer?> GetById(int id)
    {
        logger.LogInformation("Buscando cliente por Id: {CustomerId}", id);
        var customer = await context.Customers.FirstOrDefaultAsync(c => c.CustomerId == id);

        if (customer == null)
        {
            logger.LogWarning("Cliente não encontrado. Id: {CustomerId}", id);
            throw new InvalidOperationException($"Cliente com Id {id} não encontrado.");
        }

        return customer;
    }

    public async Task<Customer?> UpdateCustomer(int id, Customer customer)
    {
        logger.LogInformation("Atualizando cliente. Id: {CustomerId}", id);

        var existingCustomer = await context.Customers.FindAsync(id);

        if (existingCustomer is null)
        {
            logger.LogWarning("Cliente não encontrado para atualização. Id: {CustomerId}", id);
            throw new InvalidOperationException($"Cliente com Id {id} não encontrado.");
        }

        var emailExists = await context.Customers
           .AnyAsync(c => c.Email == customer.Email && c.CustomerId != id);

        if (emailExists)
        {
            logger.LogWarning("Tentativa de atualizar cliente com e-mail já cadastrado. Email: {Email}", customer.Email);

            throw new InvalidOperationException("E-mail já existe.");
        }

        existingCustomer.Name = customer.Name;
        existingCustomer.Phone = customer.Phone;
        existingCustomer.Email = customer.Email;
        existingCustomer.Active = customer.Active;
        existingCustomer.Cpf = customer.Cpf;

        await context.SaveChangesAsync();

        logger.LogInformation("Cliente atualizado com sucesso. Id: {CustomerId}", id);

        return existingCustomer;
    }

    public async Task<Customer> ToggleActive(int id)
    {
        logger.LogInformation("Alterando status do cliente. Id: {CustomerId}", id);

        var customer = await context.Customers.FindAsync(id);

        if (customer == null)
        {
            logger.LogWarning("Cliente não encontrado para alteração de status. Id: {CustomerId}", id);
            throw new InvalidOperationException($"Cliente com Id {id} não encontrado.");
        }

        customer.Active = !customer.Active;

        await context.SaveChangesAsync();

        logger.LogInformation("Status do cliente alterado. Id: {CustomerId}, Ativo: {Active}", id, customer.Active);

        return customer;
    }
}