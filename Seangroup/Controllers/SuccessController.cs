using Microsoft.AspNetCore.Mvc;

namespace Seangroup.Controllers
{
    public class SuccessController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
