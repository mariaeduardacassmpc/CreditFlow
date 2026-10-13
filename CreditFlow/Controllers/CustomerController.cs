using Application.Dtos.Customers;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class CustomersController(ICustomerService customerService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto, CancellationToken cancellationToken)
    {
        var customer = await customerService.CreateCustomer(dto, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = customer.CustomerId }, customer);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await customerService.GetAllCustomers(page, pageSize, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var customer = await customerService.GetById(id, cancellationToken);

        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerDto dto, CancellationToken cancellationToken)
    {
        var customer = await customerService.UpdateCustomer(id, dto, cancellationToken);

        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpPatch("{id:int}/active")]
    public async Task<IActionResult> SetActive(int id,[FromBody] SetActiveRequest request, CancellationToken cancellationToken)
    {
        var customer = await customerService.SetActive(id, request.Active, cancellationToken);

        return customer is null ? NotFound() : Ok(customer);
    }
}

public record SetActiveRequest(bool Active);