using System;
using System.Collections.Generic;
using System.Linq;
using OnlineStore.Domain;

namespace OnlineStore.App;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("=== Modern Programming Course: Lab 4 ===");
        Console.WriteLine("Student: Коломієць Даніл Сергійович | Group: 401-ТК | Variant: 1\n");

        // 1. Створення тестових покупців та товарів
        var addr = new Address("Полтава", "Соборна", "10", "36000");

        var customer1 = new Customer { Name = "Даніл Коломієць", Email = "danil@example.com", ShippingAddress = addr };
        var customer2 = new Customer { Name = "Олена Петренко", Email = "olena@example.com", ShippingAddress = addr };
        var customer3 = new Customer { Name = "Іван Коваленко", Email = "ivan@example.com", ShippingAddress = addr };
        var customer4 = new Customer { Name = "Марія Сидоренко", Email = "maria@example.com", ShippingAddress = addr };
        var customer5 = new Customer { Name = "Тарас Шевченко", Email = "taras@example.com", ShippingAddress = addr };

        var laptop = new Product(Guid.NewGuid(), "Ноутбук Lenovo", 25000m);
        var phone = new Product(Guid.NewGuid(), "Смартфон Samsung", 18000m);
        var monitor = new Product(Guid.NewGuid(), "Монітор 27\"", 8500m);
        var mouse = new Product(Guid.NewGuid(), "Миша Logitech", 1200m);
        var keyboard = new Product(Guid.NewGuid(), "Клавіатура Keychron", 3500m);
        var headphones = new Product(Guid.NewGuid(), "Навушники Sony", 4200m);
        var cable = new Product(Guid.NewGuid(), "Кабель USB-C", 300m);

        // 2. Створення InMemoryRepository та заповнення тестовими замовленнями (20 шт)
        var orderRepository = new InMemoryRepository<Order>();
        var random = new Random(42);
        var customers = new[] { customer1, customer2, customer3, customer4, customer5 };
        var products = new[] { laptop, phone, monitor, mouse, keyboard, headphones, cable };

        var cardPayment = new CardPayment("4149-xxxx-xxxx-1234");

        for (int i = 0; i < 20; i++)
        {
            var selectedCustomer = customers[random.Next(customers.Length)];
            var order = new Order(selectedCustomer);

            // Додаємо від 1 до 3 випадкових товарів у замовлення
            int itemsCount = random.Next(1, 4);
            for (int j = 0; j < itemsCount; j++)
            {
                var prod = products[random.Next(products.Length)];
                order.AddItem(prod, random.Next(1, 4));
            }

            // Випадково оплачуємо деякі замовлення
            if (i % 2 == 0)
            {
                order.Pay(cardPayment);
            }

            orderRepository.Add(order);
        }

        var orders = orderRepository.GetAll();
        Console.WriteLine($"Усього згенеровано замовлень у репозиторії: {orders.Count}\n");

        // --- ВИКОНАННЯ 10 LINQ-ЗАПИТІВ (Варіант 1) ---

        // 1. Усі orders з Total > 1000
        var ordersOver1000 = orders.Where(o => o.TotalAmount > 1000m).ToList();
        Console.WriteLine($"1. Кількість замовлень з сумою > 1000: {ordersOver1000.Count}");

        // 2. Топ-5 найдорожчих orders
        var top5ExpensiveOrders = orders.OrderByDescending(o => o.TotalAmount).Take(5);
        Console.WriteLine("\n2. Топ-5 найдорожчих замовлень:");
        foreach (var o in top5ExpensiveOrders)
        {
            Console.WriteLine($"   - Order #{o.Id.ToString()[..8]} | Клієнт: {o.Customer.Name} | Сума: {o.TotalAmount:C}");
        }

        // 3. Загальна сума продажів
        decimal totalSales = orders.Sum(o => o.TotalAmount);
        Console.WriteLine($"\n3. Загальна сума всіх замовлень: {totalSales:C}");

        // 4. Середня вартість order
        decimal averageOrderValue = orders.Average(o => o.TotalAmount);
        Console.WriteLine($"4. Середня вартість замовлення: {averageOrderValue:C}");

        // 5. Кількість orders кожного customer через GroupBy
        var ordersPerCustomer = orders
            .GroupBy(o => o.Customer.Name)
            .Select(g => new { CustomerName = g.Key, Count = g.Count() });

        Console.WriteLine("\n5. Кількість замовлень за покупцями (GroupBy):");
        foreach (var item in ordersPerCustomer)
        {
            Console.WriteLine($"   - {item.CustomerName}: {item.Count} замовлень");
        }

        // 6. Усі products з усіх orders через SelectMany
        var allOrderedProducts = orders
            .SelectMany(o => o.Items)
            .Select(item => item.Product)
            .DistinctBy(p => p.Id)
            .ToList();

        Console.WriteLine($"\n6. Унікальних продуктів, куплених у замовленнях (SelectMany): {allOrderedProducts.Count}");

        // 7. Топ-3 products за проданою quantity
        var top3ProductsByQuantity = orders
            .SelectMany(o => o.Items)
            .GroupBy(item => item.Product.Name)
            .Select(g => new { ProductName = g.Key, TotalQuantity = g.Sum(i => i.Quantity) })
            .OrderByDescending(p => p.TotalQuantity)
            .Take(3);

        Console.WriteLine("\n7. Топ-3 найпопулярніших товари за кількістю:");
        foreach (var p in top3ProductsByQuantity)
        {
            Console.WriteLine($"   - {p.ProductName}: продано {p.TotalQuantity} шт.");
        }

        // 8. Customers, які витратили більше 5000
        var VIPCustomers = orders
            .GroupBy(o => o.Customer)
            .Select(g => new { Customer = g.Key, TotalSpent = g.Sum(o => o.TotalAmount) })
            .Where(c => c.TotalSpent > 5000m);

        Console.WriteLine("\n8. Покупці, які витратили більше 5000 грн:");
        foreach (var c in VIPCustomers)
        {
            Console.WriteLine($"   - {c.Customer.Name}: витрачено {c.TotalSpent:C}");
        }

        // 9. Перевірити через Any, чи існує order > 10000
        bool hasHugeOrder = orders.Any(o => o.TotalAmount > 10000m);
        Console.WriteLine($"\n9. Чи є хоча б одне замовлення > 10000 грн (Any): {hasHugeOrder}");

        // 10. Створити Dictionary<CustomerId, decimal> із загальними витратами customer
        Dictionary<Guid, decimal> customerTotalSpentDict = orders
            .GroupBy(o => o.Customer.Id)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(o => o.TotalAmount)
            );

        Console.WriteLine($"\n10. Створено Словник (Dictionary) витрат для {customerTotalSpentDict.Count} покупців.");
    }
}