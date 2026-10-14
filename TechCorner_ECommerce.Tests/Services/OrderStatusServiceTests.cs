using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Models;
using TechCorner_ECommerce.Models.Enums;
using TechCorner_ECommerce.Services;
using TechCorner_ECommerce.Tests.TestSupport;

namespace TechCorner_ECommerce.Tests.Services;

public class OrderStatusServiceTests {
    [Fact]
    public async Task ChangeStatusAsync_WhenTransitionIsValid_UpdatesOrderPaymentAndHistory() {
        using var db = TestDb.CreateContext();
        SeedOrder(db, "ORD-001", OrderStatus.Pending, PaymentStatus.Pending);
        var service = new OrderStatusService(db);

        var result = await service.ChangeStatusAsync("ORD-001", OrderStatus.Confirmed, PaymentStatus.Paid, "admin@example.com");

        Assert.True(result.Succeeded);
        var order = db.Orders.Single(x => x.OrderCode == "ORD-001");
        Assert.Equal(OrderStatus.Confirmed, order.Status);

        var payment = db.Payments.Single(x => x.OrderId == order.Id);
        Assert.Equal(PaymentStatus.Paid, payment.Status);
        Assert.NotNull(payment.PaidAt);

        var history = Assert.Single(db.OrderStatusHistories);
        Assert.Equal(order.Id, history.OrderId);
        Assert.Equal(OrderStatus.Pending, history.OldStatus);
        Assert.Equal(OrderStatus.Confirmed, history.NewStatus);
        Assert.Equal("admin@example.com", history.ChangedBy);
    }

    [Fact]
    public async Task ChangeStatusAsync_WhenTransitionIsInvalid_DoesNotChangeDataOrCreateHistory() {
        using var db = TestDb.CreateContext();
        SeedOrder(db, "ORD-002", OrderStatus.Shipping, PaymentStatus.Pending);
        var service = new OrderStatusService(db);

        var result = await service.ChangeStatusAsync("ORD-002", OrderStatus.Cancelled, PaymentStatus.Refunded, "admin@example.com");

        Assert.False(result.Succeeded);
        Assert.Contains("Cannot change order status", result.Message);

        var order = db.Orders.Single(x => x.OrderCode == "ORD-002");
        Assert.Equal(OrderStatus.Shipping, order.Status);
        Assert.Equal(PaymentStatus.Pending, db.Payments.Single(x => x.OrderId == order.Id).Status);
        Assert.Empty(db.OrderStatusHistories);
    }

    [Fact]
    public async Task ChangeStatusAsync_WhenStatusIsSame_DoesNotChangeDataOrCreateHistory() {
        using var db = TestDb.CreateContext();
        SeedOrder(db, "ORD-003", OrderStatus.Pending, PaymentStatus.Pending);
        var service = new OrderStatusService(db);

        var result = await service.ChangeStatusAsync("ORD-003", OrderStatus.Pending, PaymentStatus.Paid, "admin@example.com");

        Assert.False(result.Succeeded);
        Assert.Equal("Order is already Pending.", result.Message);
        Assert.Equal(OrderStatus.Pending, db.Orders.Single(x => x.OrderCode == "ORD-003").Status);
        Assert.Equal(PaymentStatus.Pending, db.Payments.Single().Status);
        Assert.Empty(db.OrderStatusHistories);
    }

    private static void SeedOrder(AppDbContext db, string orderCode, OrderStatus status, PaymentStatus paymentStatus) {
        var order = new Order {
            OrderCode = orderCode,
            ReceiverName = "Receiver",
            ReceiverEmail = "receiver@example.com",
            ReceiverPhone = "0900000000",
            ShippingAddress = "Address",
            OrderDate = DateTime.Now,
            TotalPrice = 100m,
            Status = status,
            Payments = new List<Payment> {
                new Payment {
                    PaymentMethod = "COD",
                    Status = paymentStatus,
                    CreatedAt = DateTime.Now
                }
            }
        };

        db.Orders.Add(order);
        db.SaveChanges();
    }
}
