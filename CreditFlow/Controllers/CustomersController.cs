using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CustomerController(CustomerService customerService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateCustomer(Customer customer)
    {
        var createdCustomer = await customerService.CreateCustomer(customer);

        return Ok(createdCustomer);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var customers = await customerService.GetAllCustomers();

        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await customerService.GetById(id);

        if (customer is null)
            return NotFound();

        return Ok(customer);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, Customer customer)
    {
        var updatedCustomer = await customerService.UpdateCustomer(id, customer);

        if (updatedCustomer is null)
            return NotFound();

        return Ok(updatedCustomer);
    }

    [HttpPatch("{id}/active")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var customer = await customerService.ToggleActive(id);

        return Ok((customer));
    }
}