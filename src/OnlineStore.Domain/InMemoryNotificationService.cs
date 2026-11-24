namespace OnlineStore.Domain;

public sealed class InMemoryNotificationService : INotificationService
{
    private readonly List<string> _messages = [];

    public IReadOnlyList<string> Messages => _messages.AsReadOnly();

    public void Notify(string message)
    {
        _messages.Add(message);
    }
}
