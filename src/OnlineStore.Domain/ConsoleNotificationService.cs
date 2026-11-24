namespace OnlineStore.Domain;

public sealed class ConsoleNotificationService : INotificationService
{
    public void Notify(string message)
    {
        Console.WriteLine($"[Notification -> Console]: {message}");
    }
}
