using System.Text.Encodings.Web;
using System.Text.Json;

namespace OnlineStore.Domain;

public sealed class JsonOrderReportStorage
{
    private readonly JsonSerializerOptions _options;

    public JsonOrderReportStorage(bool writeIndented = true)
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = writeIndented,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }

    public async Task SaveAsync(string filePath, IEnumerable<OrderReport> reports)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(reports);

        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(reports, _options);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<IReadOnlyList<OrderReport>> LoadAsync(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Файл звіту не знайдено за шляхом: {filePath}", filePath);
        }

        string json = await File.ReadAllTextAsync(filePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            throw new JsonException("Файл звіту порожній.");
        }

        var reports = JsonSerializer.Deserialize<List<OrderReport>>(json, _options);
        return reports ?? throw new JsonException("Не вдалося десеріалізувати список замовлень (отримано null).");
    }
}
