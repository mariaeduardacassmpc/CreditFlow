using Microsoft.AspNetCore.Mvc;

namespace CreditFlow.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
