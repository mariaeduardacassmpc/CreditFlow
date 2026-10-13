using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class CreditFlowDbContext(
    DbContextOptions<CreditFlowDbContext> options)
    : DbContext(options)
{
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

        modelBuilder.Entity<CreditRequest>()
            .Property(x => x.Status)
            .HasConversion<string>();

        modelBuilder.Entity<CreditRequest>()
            .Property(x => x.Status)
            .HasConversion(
                status => status == Status.Rejected ? "Reprovado" :
                          status == Status.Approved ? "Aprovado" :
                          status == Status.Pending ? "Pendente" :
                          status.ToString(),
                value => value == "Reprovado" ? Status.Rejected :
                         value == "Aprovado" ? Status.Approved :
                 value == "Pendente" ? Status.Pending :
                 Enum.Parse<Status>(value));
    }
}
