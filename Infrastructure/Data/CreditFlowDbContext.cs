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
    public DbSet<CreditRequest> CreditRequests => Set<CreditRequest>();
    public DbSet<User> Users => Set<User>();
    public DbSet<CreditAnalysis> CreditAnalyses => Set<CreditAnalysis>();
    public DbSet<CreditRuleResult> CreditRuleResults => Set<CreditRuleResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>()
            .Property(x => x.BirthDate)
            .HasColumnType("date");
    }
}