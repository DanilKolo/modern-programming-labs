using System;
using OnlineStore.Domain;

namespace OnlineStore.App;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("=== Modern Programming Course: Lab 2 ===");
        Console.WriteLine("Student: Коломієць Даніл Сергійович | Group: 401-ТК | Variant: 1\n");

        var customer = new Customer(Guid.NewGuid(), "Коломієць Даніл Сергійович", "kolomietc14@gmail.com");
        var laptop = new Product(Guid.NewGuid(), "Ноутбук Lenovo IdeaPad Slim 3", 22500m);
        var mouse = new Product(Guid.NewGuid(), "Бездротова миша", 750m);

        var order = new Order(Guid.NewGuid(), customer);
        order.AddItem(laptop, 1);
        order.AddItem(mouse, 2);

        Console.WriteLine($"Замовлення #{order.Id} для клієнта: {order.Customer.FullName}");
        Console.WriteLine("Товари у замовленні:");
        foreach (var item in order.Items)
        {
            Console.WriteLine($" - {item.Product.Name} x{item.Quantity} = {item.TotalPrice:C}");
        }
        Console.WriteLine($"Загальна сума: {order.TotalAmount:C}\n");

        Console.WriteLine("--- Демонстрація поліморфічного виклику оплати ---");
        IPaymentMethod payment1 = new CardPayment("4149-xxxx-xxxx-1234");
        order.Pay(payment1);
        Console.WriteLine($"Стан замовлення після оплати: {order.Status}\n");

        var order2 = new Order(Guid.NewGuid(), customer);
        order2.AddItem(mouse, 1);

        IPaymentMethod payment2 = new BankTransferPayment("UA123456789012345678901234567");
        order2.Pay(payment2);
        Console.WriteLine($"Стан другого замовлення: {order2.Status}\n");

        Console.WriteLine("--- Перевірка правил валідації (Спроба повторної оплати) ---");
        try
        {
            order.Pay(payment1);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Успішно перехоплено помилку бізнес-логіки: {ex.Message}");
        }
    }
}