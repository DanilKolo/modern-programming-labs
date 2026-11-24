using System.Diagnostics;
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

string externalApiBaseUrl = configuration["ExternalApi:BaseUrl"]
    ?? throw new InvalidOperationException("Параметр ExternalApi:BaseUrl не знайдено у конфігурації.");

// 2. Демонстрація ЛР №7: формування та збереження звіту замовлень у JSON
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
var storage = new JsonOrderReportStorage(writeIndented);
await storage.SaveAsync(fullReportPath, orders.Select(OrderReport.FromOrder));

// 3. Демонстрація ЛР №8: зовнішній довідник контактів покупців через HttpClient + Task.WhenAll
Console.WriteLine("=== Контакти покупців (зовнішній довідник) ===");
Console.WriteLine("Loading external contacts...");

using var httpClient = new HttpClient
{
    BaseAddress = new Uri(externalApiBaseUrl)
};
var contactsClient = new ExternalContactsClient(httpClient);
int[] userIds = [1, 2, 3];

try
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

    // Одночасне завантаження через Task.WhenAll
    var concurrentSw = Stopwatch.StartNew();
    IReadOnlyList<ExternalUser> concurrentUsers = await contactsClient.GetUsersConcurrentlyAsync(userIds, cts.Token);
    concurrentSw.Stop();

    Console.WriteLine($"Received: {concurrentUsers.Count} (за {concurrentSw.ElapsedMilliseconds} мс)");
    foreach (ExternalUser user in concurrentUsers)
    {
        Console.WriteLine($"{user.Id} | {user.Name} | {user.Email}");
    }

    Console.WriteLine("\nПримітка (Варіант 1): ExternalUser — це лише зовнішній DTO для довідкових контактів із API.");
    Console.WriteLine("Він не містить бізнес-правил, адреси доставки чи історії замовлень, на відміну від сутності Customer, тому замовлення автоматично не створюються.");

    // Додаткове завдання: порівняння послідовних запитів і Task.WhenAll
    Console.WriteLine("\n=== Додаткове завдання: порівняння швидкодії ===");
    var sequentialSw = Stopwatch.StartNew();
    foreach (int id in userIds)
    {
        await contactsClient.GetUserAsync(id, cts.Token);
    }
    sequentialSw.Stop();

    Console.WriteLine($"Послідовно (3 запити по черзі): {sequentialSw.ElapsedMilliseconds} мс");
    Console.WriteLine($"Одночасно (Task.WhenAll):       {concurrentSw.ElapsedMilliseconds} мс");
}
catch (OperationCanceledException)
{
    Console.WriteLine("[Помилка] Час очікування відповіді від зовнішнього API вичерпано або операцію скасовано.");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"[Помилка мережі] Не вдалося отримати контакти: {ex.Message}");
}
catch (JsonException ex)
{
    Console.WriteLine($"[Помилка JSON] Некоректний формат відповіді сервера: {ex.Message}");
}

// 4. Демонстрація обробки мережевої помилки та тайм-ауту без падіння програми
Console.WriteLine("\n=== Перевірка обробки помилок мережі та скасування ===");
try
{
    using var invalidHostClient = new HttpClient
    {
        BaseAddress = new Uri("https://non-existent-domain-for-lab08.invalid/")
    };
    var brokenContactsClient = new ExternalContactsClient(invalidHostClient);
    using var errorCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

    await brokenContactsClient.GetUserAsync(1, errorCts.Token);
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"[Очікувано] HttpRequestException оброблено: {ex.Message}");
}
catch (OperationCanceledException)
{
    Console.WriteLine("[Очікувано] OperationCanceledException оброблено (тайм-аут).");
}
