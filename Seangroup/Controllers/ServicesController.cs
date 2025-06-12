using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Seangroup.Controllers
{
    public class ServicesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

    }
}
