using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TechCorner_ECommerce.Models;

namespace TechCorner_ECommerce.Controllers {
    public class HomeController : Controller {
        public IActionResult Index() {
            return RedirectToAction("Index", "Product");
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
