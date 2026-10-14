using TechCorner_ECommerce.Services;
using TechCorner_ECommerce.Tests.TestSupport;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Tests.Services;

public class CartServiceTests {
    [Fact]
    public void AddToCart_WhenQuantityExceedsStock_ReturnsErrorAndDoesNotAddItem() {
        using var db = TestDb.CreateContext();
        var product = ProductSeed.AddProduct(db, stockQuantity: 2);
        var service = new CartService(db);
        var cart = new List<CartItemVM>();

        var result = service.AddToCart(cart, product.Id, quantity: 3);

        Assert.False(result.Success);
        Assert.Equal(2, result.Available);
        Assert.Empty(result.Cart);
        Assert.Contains("only has 2 item", result.Message);
    }

    [Fact]
    public void AddToCart_WhenProductIsAvailable_AddsProductSnapshotToSessionCart() {
        using var db = TestDb.CreateContext();
        var product = ProductSeed.AddProduct(db, stockQuantity: 5, price: 250m, productName: "Razer Viper");
        var service = new CartService(db);
        var cart = new List<CartItemVM>();

        var result = service.AddToCart(cart, product.Id, quantity: 2);

        Assert.True(result.Success);
        var item = Assert.Single(result.Cart);
        Assert.Equal(product.Id, item.ProductId);
        Assert.Equal("Razer Viper", item.ProductName);
        Assert.Equal(250m, item.Price);
        Assert.Equal(2, item.Quantity);
        Assert.Equal("/images/mouse.png", item.ImageUrl);
        Assert.Contains(item.Attributes, attr => attr.Name == "Color" && attr.Value == "Black");
    }

    [Fact]
    public void UpdateQuantity_WhenProductWasDeleted_RemovesItemFromCart() {
        using var db = TestDb.CreateContext();
        var product = ProductSeed.AddProduct(db, stockQuantity: 5);
        var service = new CartService(db);
        var cart = new List<CartItemVM> {
            new CartItemVM {
                ProductId = product.Id,
                ProductName = "Gaming Mouse",
                Price = product.Price,
                Quantity = 1
            }
        };

        product.IsDeleted = true;
        db.SaveChanges();

        var result = service.UpdateQuantity(cart, product.Id, quantity: 2);

        Assert.False(result.Success);
        Assert.True(result.Removed);
        Assert.Empty(result.Cart);
        Assert.Equal("Product is no longer available.", result.Message);
    }
}
