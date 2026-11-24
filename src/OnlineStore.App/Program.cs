using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OnlineStore.Domain;

Console.OutputEncoding = Encoding.UTF8;

// 1. Composition Root: налаштування DI-контейнера, конфігурації та журналювання
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

string configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
builder.Configuration.AddJsonFile(configPath, optional: false, reloadOnChange: false);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
});

// Реєстрація залежностей у IServiceCollection
builder.Services.AddSingleton<INotificationService, ConsoleNotificationService>();
builder.Services.AddTransient<OrderApplicationService>();

using IHost host = builder.Build();

IConfiguration configuration = host.Services.GetRequiredService<IConfiguration>();
ILoggerFactory loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
OrderApplicationService orderAppService = host.Services.GetRequiredService<OrderApplicationService>();

// 2. Зчитування параметрів конфігурації (ЛР №7-8)
string relativeFilePath = configuration["Storage:FilePath"]
    ?? throw new InvalidOperationException("Параметр Storage:FilePath не знайдено у конфігурації.");
bool writeIndented = bool.TryParse(configuration["Storage:WriteIndented"], out bool parsed) && parsed;
string fullReportPath = Path.Combine(AppContext.BaseDirectory, relativeFilePath);

string externalApiBaseUrl = configuration["ExternalApi:BaseUrl"]
    ?? throw new InvalidOperationException("Параметр ExternalApi:BaseUrl не знайдено у конфігурації.");

// 3. Демонстрація ЛР №10: SOLID, Dependency Injection та ILogger<T>
Console.WriteLine("=== Демонстрація ЛР №10: SOLID, DI та ILogger<T> ===");

var customer1 = new Customer { Name = "Олена Коваленко", Email = "olena@example.com" };
var customer2 = new Customer { Name = "Максим Бондаренко", Email = "maksym@example.com" };
var customer3 = new Customer { Name = "Ірина Шевченко", Email = "iryna@example.com" };

var laptop = new Product(Guid.NewGuid(), "Ноутбук Lenovo IdeaPad", 28500m);
var mouse = new Product(Guid.NewGuid(), "Миша бездротова", 950m);
var keyboard = new Product(Guid.NewGuid(), "Механічна клавіатура", 3200m);
var monitor = new Product(Guid.NewGuid(), "Монітор 27\" 170Hz", 8400m);

// Сценарій 1: Успішна оплата через прикладний сервіс із DI-контейнера
var order1 = new Order(customer1);
order1.AddItem(laptop, 1);
order1.AddItem(mouse, 1);
orderAppService.PayOrder(order1, new CardPayment("4111-1111-1111-1234"));

// Сценарій 2: Спроба оплати скасованого замовлення (фіксується Warning, виняток зберігається)
var cancelledOrder = new Order(customer3);
cancelledOrder.AddItem(keyboard, 1);
cancelledOrder.Cancel();

try
{
    orderAppService.PayOrder(cancelledOrder, new CardPayment("4111-1111-1111-9999"));
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"[Очікуваний виняток перехоплено]: {ex.Message}");
}

// Сценарій 3: Заміна реалізації INotificationService на InMemoryNotificationService без змін OrderApplicationService (OCP / DIP)
Console.WriteLine("\n=== Заміна INotificationService на InMemoryNotificationService (без змін сервісу) ===");
var inMemoryNotifications = new InMemoryNotificationService();
var swappableOrderService = new OrderApplicationService(
    inMemoryNotifications,
    loggerFactory.CreateLogger<OrderApplicationService>());

var order2 = new Order(customer2);
order2.AddItem(monitor, 2);
swappableOrderService.PayOrder(order2, new BankTransferPayment("UA213223130000026007233566001"));
order2.Ship();

Console.WriteLine($"Повідомлень збережено у InMemoryNotificationService: {inMemoryNotifications.Messages.Count}");
foreach (string msg in inMemoryNotifications.Messages)
{
    Console.WriteLine($" -> {msg}");
}

// 4. Збереження звіту замовлень у JSON (ЛР №7)
var order3 = new Order(customer3);
order3.AddItem(keyboard, 1);
order3.AddItem(mouse, 2);

var order5 = new Order(customer2);
order5.AddItem(laptop, 1);
order5.Pay(new CardPayment("5375-4111-2222-9876"));

var orders = new List<Order> { order1, order2, order3, cancelledOrder, order5 };
var storage = new JsonOrderReportStorage(writeIndented);
await storage.SaveAsync(fullReportPath, orders.Select(OrderReport.FromOrder));

// 5. Зовнішній довідник контактів покупців (ЛР №8)
Console.WriteLine("\n=== Контакти покупців (зовнішній довідник) ===");
using var httpClient = new HttpClient { BaseAddress = new Uri(externalApiBaseUrl) };
var contactsClient = new ExternalContactsClient(httpClient);

try
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
    var sw = Stopwatch.StartNew();
    IReadOnlyList<ExternalUser> users = await contactsClient.GetUsersConcurrentlyAsync([1, 2, 3], cts.Token);
    sw.Stop();

    Console.WriteLine($"Received: {users.Count} (за {sw.ElapsedMilliseconds} мс)");
    foreach (ExternalUser user in users)
    {
        Console.WriteLine($"{user.Id} | {user.Name} | {user.Email}");
    }
}
catch (Exception ex) when (ex is OperationCanceledException or HttpRequestException or JsonException)
{
    Console.WriteLine($"[Помилка зовнішнього API]: {ex.Message}");
}
