using Application.Dtos.Customers;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CustomerController(ICustomerService customerService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto dto, CancellationToken cancellationToken)
    {
        var createdCustomer = await customerService.CreateCustomer(dto, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = createdCustomer.CustomerId }, createdCustomer);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var customers = await customerService.GetAllCustomers( cancellationToken);

        return Ok(customers);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var customer = await customerService.GetById(id, cancellationToken);

        if (customer is null)
            return NotFound();

        return Ok(customer);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerDto dto, CancellationToken cancellationToken)
    {
        var updatedCustomer = await customerService.UpdateCustomer(id, dto, cancellationToken);

        if (updatedCustomer is null)
            return NotFound();

        return Ok(updatedCustomer);
    }

    [HttpPatch("{id:int}/active")]
    public async Task<IActionResult> ToggleActive(int id, CancellationToken cancellationToken)
    {
        var customer = await customerService.ToggleActive(id, cancellationToken);

        if (customer is null)
            return NotFound();

        return Ok(customer);
    }
}
