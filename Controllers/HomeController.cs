using Microsoft.AspNetCore.Mvc;

namespace food_delivery.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}