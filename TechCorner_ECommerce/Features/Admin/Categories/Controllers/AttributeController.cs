using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechCorner_ECommerce.Services;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Areas.Admin.Controllers {
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class AttributeController : Controller {
        private readonly IAdminCategoryService adminCategoryService;

        public AttributeController(IAdminCategoryService adminCategoryService) {
            this.adminCategoryService = adminCategoryService;
        }

        ///////////////////////// LIST /////////////////////////
        public IActionResult Index(int? categoryId) {
            return View(adminCategoryService.GetAttributeManageModel(categoryId));
        }

        ///////////////////////// ADD ATTRIBUTE /////////////////////////
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAttribute(CreateAttributeVM model) {
            var result = adminCategoryService.AddAttribute(model);

            if (!result.Succeeded) {
                return JsonError(result.Message);
            }

            return Json(new {
                success = true,
                id = result.Id,
                name = result.Name
            });
        }

        ///////////////////////// ADD ATTRIBUTE VALUE /////////////////////////
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAttributeValue(CreateAttributeValueVM model) {
            var result = adminCategoryService.AddAttributeValue(model);

            if (!result.Succeeded) {
                return JsonError(result.Message);
            }

            return Json(new {
                success = true,
                id = result.Id,
                value = result.Value
            });
        }

        ///////////////////////// DELETE ATTRIBUTE /////////////////////////
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAttribute(int id) {
            var result = adminCategoryService.DeleteAttribute(id);

            if (!result.Succeeded) {
                return JsonError(result.Message);
            }

            return Json(new { success = true });
        }

        ///////////////////////// DELETE ATTRIBUTE VALUE /////////////////////////
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAttributeValue(int id) {
            var result = adminCategoryService.DeleteAttributeValue(id);

            if (!result.Succeeded) {
                return JsonError(result.Message);
            }

            return Json(new { success = true });
        }

        private JsonResult JsonError(string message) {
            return Json(new {
                success = false,
                message
            });
        }
    }
}
