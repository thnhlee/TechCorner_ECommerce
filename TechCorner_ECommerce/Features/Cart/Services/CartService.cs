using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Services {
    public class CartService : ICartService {
        private readonly AppDbContext db;

        public CartService(AppDbContext context) {
            db = context;
        }

        ///////////////////////// ADD TO CART /////////////////////////
        public CartOperationResult AddToCart(List<CartItemVM> cart, int productId, int quantity) {
            if (productId <= 0 || quantity <= 0) {
                return Failed(cart, "Invalid cart quantity.");
            }

            var product = FindAvailableProduct(productId);

            if (product == null) {
                return Failed(cart, "Product is no longer available.");
            }

            if (product.StockQuantity <= 0) {
                return Failed(cart, $"{product.ParentProduct.Name} is out of stock.", available: 0);
            }

            var matchingItems = cart.Where(i => i.ProductId == productId).ToList();
            var item = matchingItems.FirstOrDefault();
            var currentQuantity = matchingItems.Sum(i => i.Quantity);
            var newQuantity = currentQuantity + quantity;

            // The session cart is checked against current stock before it is saved again.
            if (newQuantity > product.StockQuantity) {
                return Failed(cart, $"{product.ParentProduct.Name} only has {product.StockQuantity} item(s) in stock.", product.StockQuantity);
            }

            if (item == null) {
                cart.Add(CreateCartItem(product, quantity));
            }
            else {
                RemoveDuplicateItems(cart, matchingItems);
                item.Quantity = newQuantity;
                RefreshCartItem(item, product);
            }

            return Succeeded(cart);
        }

        ///////////////////////// REMOVE CART ITEM /////////////////////////
        public CartOperationResult RemoveFromCart(List<CartItemVM> cart, int productId) {
            if (productId <= 0) {
                return Failed(cart, "Invalid product.");
            }

            var items = cart.Where(p => p.ProductId == productId).ToList();

            foreach (var item in items) {
                cart.Remove(item);
            }

            return Succeeded(cart);
        }

        ///////////////////////// UPDATE QUANTITY /////////////////////////
        public CartOperationResult UpdateQuantity(List<CartItemVM> cart, int productId, int quantity) {
            if (productId <= 0 || quantity <= 0) {
                return Failed(cart, "Invalid cart quantity.");
            }

            var matchingItems = cart.Where(p => p.ProductId == productId).ToList();
            var item = matchingItems.FirstOrDefault();

            if (item == null) {
                return Failed(cart, "Item was not found in your cart.");
            }

            RemoveDuplicateItems(cart, matchingItems);

            var product = FindAvailableProduct(productId);

            if (product == null) {
                cart.Remove(item);
                return Failed(cart, "Product is no longer available.", available: 0, removed: true);
            }

            // Quantity update can be called directly, so stock is validated server-side.
            if (quantity > product.StockQuantity) {
                return Failed(cart, $"{product.ParentProduct.Name} only has {product.StockQuantity} item(s) in stock.", product.StockQuantity);
            }

            item.Quantity = quantity;
            RefreshCartItem(item, product);

            return Succeeded(cart);
        }

        public int GetQuantity(IEnumerable<CartItemVM> cart) {
            return cart.Sum(p => p.Quantity);
        }

        public decimal GetSubTotal(IEnumerable<CartItemVM> cart) {
            return cart.Sum(p => p.SubTotal);
        }

        private static CartOperationResult Succeeded(List<CartItemVM> cart) {
            return new CartOperationResult {
                Cart = cart
            };
        }

        private static CartOperationResult Failed(
            List<CartItemVM> cart,
            string message,
            int? available = null,
            bool removed = false) {
            return new CartOperationResult {
                Cart = cart,
                Success = false,
                Message = message,
                Available = available,
                Removed = removed
            };
        }

        private CartItemVM CreateCartItem(Models.Product product, int quantity) {
            var image = product.ParentProduct.Images
                .FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                ?? product.ParentProduct.Images.FirstOrDefault()?.ImageUrl
                ?? "";

            var attributes = product.ProductAttributeValues
                .Select(a => new AttributeVM {
                    Name = a.AttributeValue.ProductAttribute.Name,
                    Value = a.AttributeValue.Value
                }).ToList();

            return new CartItemVM {
                ProductId = product.Id,
                ProductName = product.ParentProduct.Name,
                Price = product.Price,
                Quantity = quantity,
                ImageUrl = image,
                Attributes = attributes
            };
        }

        private void RefreshCartItem(CartItemVM item, Models.Product product) {
            var freshItem = CreateCartItem(product, item.Quantity);

            item.ProductName = freshItem.ProductName;
            item.Price = freshItem.Price;
            item.ImageUrl = freshItem.ImageUrl;
            item.Attributes = freshItem.Attributes;
        }

        private static void RemoveDuplicateItems(List<CartItemVM> cart, IEnumerable<CartItemVM> matchingItems) {
            foreach (var duplicate in matchingItems.Skip(1)) {
                cart.Remove(duplicate);
            }
        }

        private Models.Product? FindAvailableProduct(int productId) {
            return db.Products
                .Include(p => p.ParentProduct)
                    .ThenInclude(pp => pp.Images)
                .Include(p => p.ProductAttributeValues)
                    .ThenInclude(pav => pav.AttributeValue)
                        .ThenInclude(av => av.ProductAttribute)
                .FirstOrDefault(p =>
                    p.Id == productId &&
                    !p.IsDeleted &&
                    !p.ParentProduct.IsDeleted);
        }
    }
}
