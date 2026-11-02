using Microsoft.AspNetCore.Mvc;
using TechCorner_ECommerce.Services;

namespace TechCorner_ECommerce.Controllers {
    public class ProductController : Controller {
        private readonly IProductCatalogService productCatalogService;

        public ProductController(IProductCatalogService productCatalogService) {
            this.productCatalogService = productCatalogService;
        }

        ///////////////////////// PRODUCT LIST /////////////////////////
        public IActionResult Index(int? cate, string? keyword, int? page) {
            const int pageSize = 9;
            var pageNumber = page ?? 1;

            ViewBag.SearchQuery = keyword;
            ViewBag.Cate = cate;

            var result = productCatalogService.GetProducts(cate, keyword, pageNumber, pageSize);

            ViewBag.Count = result.Count;
            return View(result);
        }

        ///////////////////////// PRODUCT DETAIL /////////////////////////
        public IActionResult Detail(string id) {
            var result = productCatalogService.GetProductDetail(id);

            if (result == null) {
                return Redirect("/404");
            }

            return View(result);
        }

        ///////////////////////// VARIANT POPUP DATA /////////////////////////
        [HttpGet]
        public IActionResult GetVariants(int productId) {
            var result = productCatalogService.GetVariants(productId);

            if (result == null) {
                return NotFound();
            }

            return Json(result);
        }
    }
}
