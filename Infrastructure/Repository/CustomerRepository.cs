using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CustomerRepository(CreditFlowDbContext context) : ICustomerRepository
{
    public async Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await context.Customers
            .FirstOrDefaultAsync(x => x.CustomerId == id, cancellationToken);
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await context.Customers
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<Customer?> GetByCpfAsync(string cpf, CancellationToken cancellationToken)
    {
        return await context.Customers
            .FirstOrDefaultAsync(x => x.Cpf == cpf, cancellationToken);
    }

    public async Task<(IReadOnlyList<Customer> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = context.Customers.AsNoTracking().OrderBy(c => c.CustomerId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        await context.Customers.AddAsync(customer, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}