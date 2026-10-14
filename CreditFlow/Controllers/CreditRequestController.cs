using Application.Dtos.Credit;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditFlowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CreditRequestController(
    ICreditRequestService service
) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCreditRequestDto dto, CancellationToken cancellationToken)
    {
        var creditRequest = await service.CreateCreditRequest(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = creditRequest.CreditRequestId }, creditRequest);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? customerId = null, [FromQuery] Domain.Entities.Status? status = null, CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100) return BadRequest("Página inválida. page deve ser maior que 0 e pageSize deve estar entre 1 e 100.");

        var creditRequests = await service.GetAllCreditRequests(page, pageSize, customerId, status, cancellationToken);
        return Ok(creditRequests);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var creditRequest = await service.GetById(id, cancellationToken);
        if (creditRequest is null) 
            return NotFound();

        return Ok(creditRequest);
    }
}