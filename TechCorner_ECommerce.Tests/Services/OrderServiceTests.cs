using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Models;
using TechCorner_ECommerce.Services;
using TechCorner_ECommerce.Tests.TestSupport;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Tests.Services;

public class OrderServiceTests {
    [Fact]
    public async Task PlaceOrderAsync_WhenCartIsValid_CreatesOrderSnapshotAndReducesStock() {
        using var db = TestDb.CreateContext();
        var product = ProductSeed.AddProduct(db, stockQuantity: 5, price: 150m);
        var user = new ApplicationUser {
            Id = Guid.NewGuid().ToString(),
            UserName = "buyer@example.com",
            Email = "buyer@example.com",
            FullName = "Buyer Name"
        };
        db.Users.Add(user);
        db.SaveChanges();

        var queue = new CapturingBackgroundTaskQueue();
        var service = CreateOrderService(db, queue);
        var model = new CheckoutVM {
            ReceiverName = "Original Receiver",
            Email = "receiver@example.com",
            Phone = "0900000000",
            FullAddress = "Original Address",
            PaymentMethod = "COD",
            Items = new List<CartItemVM> {
                new CartItemVM {
                    ProductId = product.Id,
                    ProductName = "Gaming Mouse",
                    Quantity = 2
                }
            }
        };

        var result = await service.PlaceOrderAsync(TestDb.PrincipalFor(user.Id), model);

        Assert.True(result.Succeeded);
        Assert.Equal("ORD-001", result.OrderCode);

        var order = Assert.Single(db.Orders);
        Assert.Equal("Original Receiver", order.ReceiverName);
        Assert.Equal("receiver@example.com", order.ReceiverEmail);
        Assert.Equal("0900000000", order.ReceiverPhone);
        Assert.Equal("Original Address", order.ShippingAddress);
        Assert.Equal(300m, order.TotalPrice);

        var savedProduct = db.Products.Single(x => x.Id == product.Id);
        Assert.Equal(3, savedProduct.StockQuantity);

        var orderDetail = Assert.Single(db.OrderDetails);
        Assert.Equal(product.Id, orderDetail.ProductId);
        Assert.Equal(2, orderDetail.Quantity);
        Assert.Equal(150m, orderDetail.Price);

        var address = Assert.Single(db.Addresses);
        address.ReceiverName = "Changed Receiver";
        address.Phone = "0999999999";
        address.FullAddress = "Changed Address";
        db.SaveChanges();

        Assert.Equal("Original Receiver", order.ReceiverName);
        Assert.Equal("0900000000", order.ReceiverPhone);
        Assert.Equal("Original Address", order.ShippingAddress);
        Assert.Single(queue.WorkItems);
    }

    [Fact]
    public async Task PlaceOrderAsync_WhenStockIsInsufficient_DoesNotCreateOrderOrReduceStock() {
        using var db = TestDb.CreateContext();
        var product = ProductSeed.AddProduct(db, stockQuantity: 1, price: 150m);
        var service = CreateOrderService(db);
        var model = new CheckoutVM {
            ReceiverName = "Receiver",
            Email = "receiver@example.com",
            Phone = "0900000000",
            FullAddress = "Address",
            PaymentMethod = "COD",
            Items = new List<CartItemVM> {
                new CartItemVM {
                    ProductId = product.Id,
                    ProductName = "Gaming Mouse",
                    Quantity = 2
                }
            }
        };

        var result = await service.PlaceOrderAsync(new ClaimsPrincipal(), model);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Message.Contains("only has 1 item"));
        Assert.Empty(db.Orders);
        Assert.Equal(1, db.Products.Single(x => x.Id == product.Id).StockQuantity);
    }

    private static OrderService CreateOrderService(AppDbContext db, CapturingBackgroundTaskQueue? queue = null) {
        queue ??= new CapturingBackgroundTaskQueue();

        return new OrderService(
            db,
            TestDb.CreateUserManager(db),
            new FixedUniqueCodeService(),
            queue,
            NullLogger<OrderService>.Instance);
    }
}
