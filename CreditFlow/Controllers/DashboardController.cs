using Microsoft.AspNetCore.Mvc;

namespace CreditFlow.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
