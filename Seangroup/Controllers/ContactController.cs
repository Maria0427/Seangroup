using Microsoft.AspNetCore.Mvc;

namespace Seangroup.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
