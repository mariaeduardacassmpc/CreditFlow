using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CreditRequestRepository(CreditFlowDbContext context) : ICreditRequestRepository
{
    public async Task<Customer?> GetCustomerAsync(int customerId)
    {
        return await context.Customers.FirstOrDefaultAsync(x => x.CustomerId == customerId);
    }

    public async Task<CreditRequest> CreateAsync(CreditRequest creditRequest)
    {
        context.CreditRequests.Add(creditRequest);

        await context.SaveChangesAsync();

        return creditRequest;
    }

    public async Task<IEnumerable<CreditRequest>> GetAllAsync()
    {
        return await context.CreditRequests.ToListAsync();
    }

    public async Task<CreditRequest?> GetByIdAsync(int id)
    {
        return await context.CreditRequests
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.CreditRequestId == id);
    }
}