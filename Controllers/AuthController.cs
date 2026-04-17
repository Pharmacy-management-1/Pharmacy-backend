using Microsoft.AspNetCore.Mvc;

namespace PharmacyApi.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
