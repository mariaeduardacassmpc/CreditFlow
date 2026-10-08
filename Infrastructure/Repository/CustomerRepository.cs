using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CustomerRepository(CreditFlowDbContext context) : ICustomerRepository
{
    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await context.Customers
            .FirstOrDefaultAsync(x => x.CustomerId == id);
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await context.Customers
            .ToListAsync();
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await context.Customers
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task AddAsync(Customer customer)
    {
        await context.Customers.AddAsync(customer);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}