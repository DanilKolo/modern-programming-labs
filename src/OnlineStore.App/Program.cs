using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using OnlineStore.Domain;

Console.OutputEncoding = Encoding.UTF8;

// 1. Зчитування конфігурації з appsettings.json
string configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile(configPath, optional: false, reloadOnChange: false)
    .Build();

string relativeFilePath = configuration["Storage:FilePath"]
    ?? throw new InvalidOperationException("Параметр Storage:FilePath не знайдено у конфігурації.");

bool writeIndented = bool.TryParse(configuration["Storage:WriteIndented"], out bool parsed) && parsed;
string fullReportPath = Path.Combine(AppContext.BaseDirectory, relativeFilePath);

// 2. Підготовка тестових даних (5 замовлень)
var customer1 = new Customer { Name = "Олена Коваленко", Email = "olena@example.com" };
var customer2 = new Customer { Name = "Максим Бондаренко", Email = "maksym@example.com" };
var customer3 = new Customer { Name = "Ірина Шевченко", Email = "iryna@example.com" };

var laptop = new Product(Guid.NewGuid(), "Ноутбук Lenovo IdeaPad", 28500m);
var mouse = new Product(Guid.NewGuid(), "Миша бездротова", 950m);
var keyboard = new Product(Guid.NewGuid(), "Механічна клавіатура", 3200m);
var monitor = new Product(Guid.NewGuid(), "Монітор 27\" 170Hz", 8400m);

var order1 = new Order(customer1);
order1.AddItem(laptop, 1);
order1.AddItem(mouse, 1);
order1.Pay(new CardPayment("4111-1111-1111-1234"));

var order2 = new Order(customer2);
order2.AddItem(monitor, 2);
order2.Pay(new BankTransferPayment("UA213223130000026007233566001"));
order2.Ship();

var order3 = new Order(customer3);
order3.AddItem(keyboard, 1);
order3.AddItem(mouse, 2);

var order4 = new Order(customer1);
order4.AddItem(monitor, 1);
order4.AddItem(keyboard, 1);
order4.Cancel();

var order5 = new Order(customer2);
order5.AddItem(laptop, 1);
order5.Pay(new CardPayment("5375-4111-2222-9876"));

var orders = new List<Order> { order1, order2, order3, order4, order5 };
List<OrderReport> reportsToSave = orders.Select(OrderReport.FromOrder).ToList();

// 3. Експорт у JSON
var storage = new JsonOrderReportStorage(writeIndented);
try
{
    await storage.SaveAsync(fullReportPath, reportsToSave);
    Console.WriteLine($"Звіт успішно збережено у файл:\n{fullReportPath}\n");
}
catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
{
    Console.WriteLine($"Помилка запису файлу: {ex.Message}");
    return;
}

// 4. Читання з JSON та підрахунок статистики (Варіант 1)
try
{
    IReadOnlyList<OrderReport> loadedReports = await storage.LoadAsync(fullReportPath);

    Console.WriteLine("=== Зчитані замовлення зі звіту ===");
    foreach (var item in loadedReports)
    {
        Console.WriteLine(
            $"ID: {item.OrderId} | Клієнт: {item.CustomerName} | Позицій: {item.ItemsCount} | Сума: {item.Total:F2} грн | Статус: {item.Status}");
    }

    decimal grandTotal = loadedReports.Sum(r => r.Total);
    Console.WriteLine("-----------------------------------");
    Console.WriteLine($"Всього замовлень у звіті: {loadedReports.Count}");
    Console.WriteLine($"Загальна сума замовлень: {grandTotal:F2} грн\n");
}
catch (Exception ex) when (ex is FileNotFoundException or UnauthorizedAccessException or IOException or JsonException)
{
    Console.WriteLine($"Помилка під час читання звіту: {ex.Message}");
}

// 5. Демонстрація коректної обробки помилок (відсутній файл + битий JSON)
Console.WriteLine("=== Перевірка обробки помилок ===");
try
{
    string missingPath = Path.Combine(AppContext.BaseDirectory, "data", "non-existent.json");
    await storage.LoadAsync(missingPath);
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"[Очікувано] Відсутній файл оброблено: {ex.Message}");
}

try
{
    string brokenJsonPath = Path.Combine(AppContext.BaseDirectory, "data", "broken.json");
    await File.WriteAllTextAsync(brokenJsonPath, "{ invalid json content ::: ");
    await storage.LoadAsync(brokenJsonPath);
}
catch (JsonException ex)
{
    Console.WriteLine($"[Очікувано] Помилку формату JSON оброблено: {ex.Message}");
}
