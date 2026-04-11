using Microsoft.AspNetCore.Mvc;

namespace TechCorner_ECommerce.Controllers {
    public class ProductController : Controller {
        public IActionResult Index() {
            return View();
        }
    }
}
