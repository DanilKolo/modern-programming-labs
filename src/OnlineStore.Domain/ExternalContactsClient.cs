using System.Net.Http.Json;

namespace OnlineStore.Domain;

public sealed class ExternalContactsClient
{
    private readonly HttpClient _httpClient;

    public ExternalContactsClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<ExternalUser?> GetUserAsync(int id, CancellationToken token = default)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "Ідентифікатор користувача має бути додатним.");
        }

        return await _httpClient.GetFromJsonAsync<ExternalUser>($"users/{id}", token);
    }

    public async Task<IReadOnlyList<ExternalUser>> GetUsersConcurrentlyAsync(
        IEnumerable<int> ids,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        Task<ExternalUser?>[] tasks = ids
            .Select(id => GetUserAsync(id, token))
            .ToArray();

        ExternalUser?[] results = await Task.WhenAll(tasks);

        return results
            .Where(user => user is not null)
            .Select(user => user!)
            .ToList();
    }
}
