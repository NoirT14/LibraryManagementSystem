using Microsoft.AspNetCore.Mvc;

namespace APIServer.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
