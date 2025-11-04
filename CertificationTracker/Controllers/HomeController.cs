using Microsoft.AspNetCore.Mvc;

namespace CertificationTracker.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}