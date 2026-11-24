using Microsoft.Extensions.Logging.Abstractions;
using OnlineStore.Domain;

namespace OnlineStore.Tests;

public sealed class OrderApplicationServiceTests
{
    private sealed class FakePaymentMethod(bool shouldSucceed = true) : IPaymentMethod
    {
        public bool Pay(decimal amount) => shouldSucceed;
    }

    private static Customer CreateTestCustomer() =>
        new() { Name = "Даніл Коломієць", Email = "danil@example.com" };

    [Fact]
    public void PayOrder_WhenPaymentSucceeds_SendsSingleNotificationToInMemoryService()
    {
        // Arrange
        var notificationService = new InMemoryNotificationService();
        var appService = new OrderApplicationService(
            notificationService,
            NullLogger<OrderApplicationService>.Instance);

        var order = new Order(CreateTestCustomer());
        order.AddItem(new Product(Guid.NewGuid(), "Ноутбук", 25000m), 1);

        // Act
        appService.PayOrder(order, new FakePaymentMethod(shouldSucceed: true));

        // Assert
        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Single(notificationService.Messages);
        Assert.Contains(order.Id.ToString(), notificationService.Messages[0]);
    }

    [Fact]
    public void PayOrder_WhenOrderIsCancelled_ThrowsAndDoesNotSendNotification()
    {
        // Arrange
        var notificationService = new InMemoryNotificationService();
        var appService = new OrderApplicationService(
            notificationService,
            NullLogger<OrderApplicationService>.Instance);

        var order = new Order(CreateTestCustomer());
        order.AddItem(new Product(Guid.NewGuid(), "Миша", 950m), 1);
        order.Cancel();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            appService.PayOrder(order, new FakePaymentMethod(shouldSucceed: true)));

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Empty(notificationService.Messages);
    }

    [Fact]
    public void PayOrder_WhenPaymentDeclined_DoesNotSendNotification()
    {
        // Arrange
        var notificationService = new InMemoryNotificationService();
        var appService = new OrderApplicationService(
            notificationService,
            NullLogger<OrderApplicationService>.Instance);

        var order = new Order(CreateTestCustomer());
        order.AddItem(new Product(Guid.NewGuid(), "Клавіатура", 3200m), 1);

        // Act
        appService.PayOrder(order, new FakePaymentMethod(shouldSucceed: false));

        // Assert
        Assert.Equal(OrderStatus.Created, order.Status);
        Assert.Empty(notificationService.Messages);
    }
}
