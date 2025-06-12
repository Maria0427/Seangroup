using Microsoft.AspNetCore.Mvc;

namespace Seangroup.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
