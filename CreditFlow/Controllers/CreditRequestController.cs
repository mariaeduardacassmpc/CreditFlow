using Application.Dtos.Credit;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditFlowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CreditRequestController(CreditRequestService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateCreditRequestDto dto)
    {
        var creditRequest = await service.CreateCreditRequest(dto);

        return Ok(creditRequest);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var creditRequests = await service.GetAllCreditRequests();

        return Ok(creditRequests);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var creditRequest = await service.GetById(id);

        if (creditRequest is null)
            return NotFound();

        return Ok(creditRequest);
    }
}