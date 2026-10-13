using Microsoft.AspNetCore.Mvc;

namespace CreditFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CreditScoreController : ControllerBase
{
    [HttpGet("{cpf}")]
    public IActionResult GetScore(string cpf)
    {
        var scores = new Dictionary<string, int>
        {
            ["52998224725"] = 742,
            ["11144477735"] = 650,
            ["93541134780"] = 480
        };

        if (!scores.TryGetValue(cpf, out var score))
        {
            return NotFound(new
            {
                message = "Score não encontrado para este cliente."
            });
        }

        return Ok(new { score });
    }
}
