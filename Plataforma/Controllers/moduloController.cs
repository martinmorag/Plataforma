using Microsoft.AspNetCore.Mvc;

namespace Plataforma.Controllers
{
    public class moduloController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
