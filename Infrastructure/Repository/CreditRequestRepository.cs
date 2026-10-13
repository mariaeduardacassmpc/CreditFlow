using Application.Common;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CreditRequestRepository(CreditFlowDbContext context) : ICreditRequestRepository
{
    public async Task<Customer?> GetCustomerAsync(int customerId, CancellationToken cancellationToken)
    {
        return await context.Customers.FirstOrDefaultAsync(x => x.CustomerId == customerId);
    }

    public async Task<CreditRequest> CreateAsync(CreditRequest creditRequest, CancellationToken cancellationToken)
    {
        context.CreditRequests.Add(creditRequest);

        await context.SaveChangesAsync();

        return creditRequest;
    }

    public async Task<PagedResult<CreditRequest>> GetAllAsync(int page, int pageSize, int? customerId, Status? status, CancellationToken cancellationToken)
    {
        var query = context.CreditRequests.AsNoTracking().AsQueryable();

        if (customerId.HasValue)
            query = query.Where(x => x.CustomerId == customerId.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<CreditRequest>(items, totalCount, page, pageSize);
    }

    public async Task<CreditRequest?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await context.CreditRequests
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.CreditRequestId == id);
    }
}