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
            ["mariana@gmail.com"] = 742,
            ["luiza@gmail.com"] = 650,
            ["joao@gmail.com"] = 480
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