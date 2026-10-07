using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class CustomerService(CreditFlowDbContext context)
{
    public async Task<Customer> CreateCustomer(Customer customer)
    {
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        return customer;
    }

    public async Task<IEnumerable<Customer>> GetAllCustomers()
    {
        return await context.Customers.ToListAsync();
    }

    public async Task<Customer?> GetById(int id)
    {
        return await context.Customers.FindAsync(id);
    }

    public async Task<Customer?> UpdateCustomer(int id, Customer customer)
    {
        var existingCustomer = await context.Customers.FindAsync(id);

        if (existingCustomer is null)
            return null;

        existingCustomer.Name = customer.Name;
        existingCustomer.Phone = customer.Phone;
        existingCustomer.City = customer.City;
        existingCustomer.Address = customer.Address;
        existingCustomer.Email = customer.Email;
        existingCustomer.Active = customer.Active;

        await context.SaveChangesAsync();

        return existingCustomer;
    }
}