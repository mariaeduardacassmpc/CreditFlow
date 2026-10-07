using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;


public class CreditFlowDbContext : DbContext
{
    public CreditFlowDbContext(
        DbContextOptions<CreditFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<User> Users => Set<User>();
}