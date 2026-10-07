using Microsoft.AspNetCore.Mvc;

namespace CreditScoreMockAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CreditScoreController : ControllerBase
{
    [HttpGet("{email}")]
    public IActionResult GetScore(string email)
    {
        var scores = new Dictionary<string, int>
        {
            ["12345678900"] = 742,
            ["98765432100"] = 650,
            ["11122233344"] = 480
        };

        if (!scores.TryGetValue(email, out var score))
        {
            return NotFound(new
            {
                message = "Score não encontrado para este cliente."
            });
        }

        return Ok(new
        {
            score
        });
    }
}