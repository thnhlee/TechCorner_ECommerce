using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechCorner_ECommerce.Data;

namespace TechCorner_ECommerce.Areas.Admin.Controllers {
    public class UserController : Controller {
        [Authorize]
        [Area("Admin")]
        public class InventoryController : Controller {
            private readonly AppDbContext db;

            public InventoryController(AppDbContext context) {
                db = context;
            }


            public IActionResult Index() {
                return View();
            }



        }
    }
}
