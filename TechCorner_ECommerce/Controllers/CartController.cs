using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.ViewModels;
using TechCorner_ECommerce.Helpers;

namespace TechCorner_ECommerce.Controllers {
    public class CartController : Controller {
        private readonly AppDbContext db;

        public CartController(AppDbContext context) {
            db = context;
        }

        public List<CartItemVM> Cart =>
            HttpContext.Session.Get<List<CartItemVM>>(MySetting.CART_KEY)
            ?? new List<CartItemVM>();

        public IActionResult Index() {
            return View(Cart);
        }

        public IActionResult AddToCart(int productId, int quantity = 1) {
            var cart = Cart;

            var item = cart.SingleOrDefault(i => i.ProductId == productId);

            if (item == null) {
                var product = db.Products
                    .Include(p => p.ParentProduct)
                        .ThenInclude(pp => pp.Images)
                    .Include(p => p.ProductAttributeValues)
                        .ThenInclude(pav => pav.AttributeValue)
                            .ThenInclude(av => av.ProductAttribute)
                    .FirstOrDefault(p => p.Id == productId);

                if (product == null) {
                    return Redirect("/404");
                }

                var image = product.ParentProduct.Images
                    .FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                    ?? product.ParentProduct.Images.FirstOrDefault()?.ImageUrl
                    ?? "";

                var attributes = product.ProductAttributeValues
                    .Select(a => new AttributeVM {
                        Name = a.AttributeValue.ProductAttribute.Name,
                        Value = a.AttributeValue.Value
                    }).ToList();

                item = new CartItemVM {
                    ProductId = product.Id,
                    ProductName = product.ParentProduct.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = image,
                    Attributes = attributes
                };

                cart.Add(item);
            }
            else {
                item.Quantity += quantity;
            }

            HttpContext.Session.Set(MySetting.CART_KEY, cart);

            return Json(new {
                success = true,
                quantity = cart.Sum(p => p.Quantity)
            });
        }

        public IActionResult RemoveCart(int productId) {
            var cart = Cart;

            var item = cart.SingleOrDefault(p => p.ProductId == productId);

            if (item != null) {
                cart.Remove(item);
                HttpContext.Session.Set(MySetting.CART_KEY, cart);
            }

            int totalQty = cart.Sum(p => p.Quantity);
            decimal subTotal = cart.Sum(p => p.SubTotal);

            return Json(new {
                success = true,
                quantity = totalQty,
                subTotal
            });
        }

        public IActionResult UpdateQuantity(int productId, int quantity) {
            var cart = Cart;

            var item = cart.SingleOrDefault(p => p.ProductId == productId);

            if (item != null) {
                item.Quantity = quantity;
                HttpContext.Session.Set(MySetting.CART_KEY, cart);
            }

            int totalQty = cart.Sum(x => x.Quantity);
            decimal totalPrice = cart.Sum(x => x.SubTotal);

            return Json(new {
                success = true,
                quantity = totalQty,
                totalPrice
            });
        }
    }
}