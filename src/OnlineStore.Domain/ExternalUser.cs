namespace OnlineStore.Domain;

public sealed record ExternalUser(
    int Id,
    string Name,
    string Email);
