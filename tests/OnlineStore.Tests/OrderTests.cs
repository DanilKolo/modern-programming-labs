using OnlineStore.Domain;

namespace OnlineStore.Tests;

public sealed class OrderTests
{
    private sealed class FakePaymentMethod(bool shouldSucceed = true) : IPaymentMethod
    {
        public int CallsCount { get; private set; }
        public decimal LastChargedAmount { get; private set; }

        public bool Pay(decimal amount)
        {
            CallsCount++;
            LastChargedAmount = amount;
            return shouldSucceed;
        }
    }

    private static Customer CreateTestCustomer() =>
        new() { Name = "Даніл Коломієць", Email = "danil@example.com" };

    // 1. Успішний сценарій: правильний підрахунок суми замовлення з двома товарами
    [Fact]
    public void TotalAmount_WithTwoProducts_CalculatesCorrectSum()
    {
        // Arrange
        var order = new Order(CreateTestCustomer());
        var laptop = new Product(Guid.NewGuid(), "Ноутбук", 25000m);
        var mouse = new Product(Guid.NewGuid(), "Миша", 1200m);

        // Act
        order.AddItem(laptop, 1);
        order.AddItem(mouse, 2);

        // Assert
        Assert.Equal(27400m, order.TotalAmount);
        Assert.Equal(2, order.Items.Count);
    }

    // 2. Параметризований тест [Theory]: відхилення кількості 0 та -1 при додаванні товару
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddItem_WhenQuantityIsZeroOrNegative_ThrowsArgumentOutOfRangeExceptionAndLeavesItemsEmpty(int invalidQuantity)
    {
        // Arrange
        var order = new Order(CreateTestCustomer());
        var product = new Product(Guid.NewGuid(), "Клавіатура", 3000m);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => order.AddItem(product, invalidQuantity));
        Assert.Empty(order.Items);
        Assert.Equal(0m, order.TotalAmount);
    }

    // 3. Успішний сценарій: перехід Order зі стану Created до Paid
    [Fact]
    public void Pay_WhenOrderIsCreatedAndHasItems_ChangesStatusToPaid()
    {
        // Arrange
        var order = new Order(CreateTestCustomer());
        order.AddItem(new Product(Guid.NewGuid(), "Монітор", 8500m), 1);
        var fakePayment = new FakePaymentMethod(shouldSucceed: true);

        // Act
        order.Pay(fakePayment);

        // Assert
        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Equal(1, fakePayment.CallsCount);
        Assert.Equal(8500m, fakePayment.LastChargedAmount);
    }

    // 4. Сценарій з помилкою: заборона повторної оплати без зміни статусу
    [Fact]
    public void Pay_WhenOrderIsAlreadyPaid_ThrowsInvalidOperationExceptionAndKeepsPaidStatus()
    {
        // Arrange
        var order = new Order(CreateTestCustomer());
        order.AddItem(new Product(Guid.NewGuid(), "Навушники", 2400m), 1);
        var fakePayment = new FakePaymentMethod(shouldSucceed: true);
        order.Pay(fakePayment);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.Pay(fakePayment));
        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Equal(1, fakePayment.CallsCount);
    }

    // 5. Перевірка події: OrderPaid виникає після успішної оплати та не виникає після помилки
    [Fact]
    public void OrderPaid_RaisedAfterSuccessfulPayment_AndNotRaisedOnFailedOrDuplicatePayment()
    {
        // Arrange
        var order = new Order(CreateTestCustomer());
        order.AddItem(new Product(Guid.NewGuid(), "SSD 1TB", 3200m), 1);

        int eventCount = 0;
        order.OrderPaid += (_, _) => eventCount++;

        var declinedPayment = new FakePaymentMethod(shouldSucceed: false);
        var approvedPayment = new FakePaymentMethod(shouldSucceed: true);

        // Act 1: відхилена оплата (подія не повинна спрацювати)
        order.Pay(declinedPayment);
        Assert.Equal(OrderStatus.Created, order.Status);
        Assert.Equal(0, eventCount);

        // Act 2: успішна оплата (подія спрацьовує 1 раз)
        order.Pay(approvedPayment);
        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Equal(1, eventCount);

        // Act 3: спроба повторної оплати кидає виняток і не викликає подію вдруге
        Assert.Throws<InvalidOperationException>(() => order.Pay(approvedPayment));
        Assert.Equal(1, eventCount);
    }

    // Додаткове завдання: перевірка виклику двох підписників на одну подію OrderPaid
    [Fact]
    public void OrderPaid_WhenMultipleSubscribersAttached_NotifiesAllSubscribers()
    {
        // Arrange
        var order = new Order(CreateTestCustomer());
        order.AddItem(new Product(Guid.NewGuid(), "Вебкамера", 1900m), 1);

        int emailNotifierCalls = 0;
        int warehouseNotifierCalls = 0;

        order.OrderPaid += (_, _) => emailNotifierCalls++;
        order.OrderPaid += (_, _) => warehouseNotifierCalls++;

        // Act
        order.Pay(new FakePaymentMethod(shouldSucceed: true));

        // Assert
        Assert.Equal(1, emailNotifierCalls);
        Assert.Equal(1, warehouseNotifierCalls);
    }
}
