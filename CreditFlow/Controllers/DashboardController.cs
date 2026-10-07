using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditFlowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController(
    DashboardService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var dashboard = await service.GetDashboard();

        return Ok(dashboard);
    }
}