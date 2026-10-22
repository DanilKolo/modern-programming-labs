using System;
using OnlineStore.Domain;

namespace OnlineStore.App;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("=== Modern Programming Course: Lab 3 ===");
        Console.WriteLine("Student: Коломієць Даніл Сергійович | Group: 401-ТК | Variant: 1\n");

        // 1. Використання record (Value Object) та required / init / nullable properties
        var address = new Address("Полтава", "вул. Соборна", "14A", "36000");

        var customer = new Customer
        {
            Name = "Коломієць Даніл Сергійович",
            Email = "kolomietc14@gmail.com",
            ShippingAddress = address,
            PhoneNumber = "+380991234567" // Nullable property
        };

        Console.WriteLine($"Покупець: {customer.Name} ({customer.Email})");
        Console.WriteLine($"Телефон: {customer.PhoneNumber ?? "Не вказано"}");
        Console.WriteLine($"Адреса доставки: м. {customer.ShippingAddress.City}, {customer.ShippingAddress.Street}, буд. {customer.ShippingAddress.Building}\n");

        // 2. Створення замовлення та додавання товарів
        var laptop = new Product(Guid.NewGuid(), "Ноутбук Lenovo IdeaPad Slim 3", 22500m);
        var mouse = new Product(Guid.NewGuid(), "Бездротова миша", 450m);

        var order1 = new Order(customer);
        order1.AddItem(mouse, 1); // 450 грн (< 500)

        var order2 = new Order(customer);
        order2.AddItem(laptop, 1); // 22 500 грн (>= 5000)

        // 3. Демонстрація Pattern Matching: Relational patterns (Категорії замовлень)
        Console.WriteLine("--- Демонстрація Relational Patterns (Категорія замовлення) ---");
        Console.WriteLine($"Замовлення 1 (Сума: {order1.TotalAmount:C}): Категорія = {OrderAnalysisService.GetOrderCategory(order1.TotalAmount)}");
        Console.WriteLine($"Замовлення 2 (Сума: {order2.TotalAmount:C}): Категорія = {OrderAnalysisService.GetOrderCategory(order2.TotalAmount)}\n");

        // 4. Демонстрація Switch Expression: Опис статусів
        Console.WriteLine("--- Демонстрація Switch Expression (Статуси) ---");
        Console.WriteLine($"Поточний статус замовлення 2: {order2.Status}");
        Console.WriteLine($"Опис статусу: {OrderAnalysisService.GetStatusDescription(order2.Status)}");

        // Оплата замовлення
        IPaymentMethod cardPayment = new CardPayment("4149-xxxx-xxxx-1234");
        order2.Pay(cardPayment);

        Console.WriteLine($"Статус після оплати: {order2.Status}");
        Console.WriteLine($"Опис статусу: {OrderAnalysisService.GetStatusDescription(order2.Status)}\n");

        // 5. Демонстрація Property Pattern: Безкоштовна доставка (Paid + Total >= 2000)
        Console.WriteLine("--- Демонстрація Property Pattern (Free Delivery Rule) ---");
        bool isOrder1FreeDelivery = OrderAnalysisService.IsEligibleForFreeDelivery(order1);
        bool isOrder2FreeDelivery = OrderAnalysisService.IsEligibleForFreeDelivery(order2);

        Console.WriteLine($"Замовлення 1 (Оплачено: {order1.Status == OrderStatus.Paid}, Сума: {order1.TotalAmount:C}) -> Безкоштовна доставка: {isOrder1FreeDelivery}");
        Console.WriteLine($"Замовлення 2 (Оплачено: {order2.Status == OrderStatus.Paid}, Сума: {order2.TotalAmount:C}) -> Безкоштовна доставка: {isOrder2FreeDelivery}");
    }
}