using Microsoft.AspNetCore.Mvc;

namespace Seangroup.Controllers
{
    public class PortfolioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
