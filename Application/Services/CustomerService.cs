using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class CustomerService(ICustomerRepository customerRepository)
{
    public async Task<Customer> CreateCustomer(Customer customer)
    {
        return await customerRepository.CreateAsync(customer);
    }

    public async Task<IEnumerable<Customer>> GetAllCustomers()
    {
        return await customerRepository.GetAllAsync();
    }

    public async Task<Customer?> GetById(int id)
    {
        return await customerRepository.GetByIdAsync(id);
    }

    public async Task<Customer?> UpdateCustomer(int id, Customer customer)
    {
        return await customerRepository.UpdateAsync(id, customer);
    }
}