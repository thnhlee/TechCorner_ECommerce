using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TechCorner_ECommerce.Models;

namespace TechCorner_ECommerce.Controllers {
    public class HomeController : Controller {

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger) {
            _logger = logger;
        }
        public IActionResult Index() {
            return View();
            //return RedirectToAction("Index", "Product");
        }

        public IActionResult About() {
            return View();
        }

        [Route("/404")]
        public IActionResult PageNotFound() {
            return View();
        }

        public IActionResult Privacy() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
