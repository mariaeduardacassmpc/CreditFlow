using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CreditFlowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CreditRequestController : ControllerBase
{
    [HttpPost]
    public IActionResult Create(CreditRequest request)
    {
        return Ok(request);
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok();
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        return Ok(id);
    }
}