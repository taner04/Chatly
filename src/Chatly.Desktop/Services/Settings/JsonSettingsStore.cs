using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Chatly.Desktop.Abstraction.Settings;
using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.Services.Settings;

[SingletonService(typeof(ISettingsStore))]
internal sealed partial class JsonSettingsStore(
    ISettingsDirectoryProvider directoryProvider,
    ILogger<JsonSettingsStore> logger) : ISettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };

    private readonly string _rootDirectory = Path.Combine(directoryProvider.GetRootDirectory(), "Chatly", "Settings");

    public void SaveSettings<T>(T settings)
        where T : class, ISettingsGroup
    {
        ArgumentNullException.ThrowIfNull(settings);

        var filePath = GetFilePath<T>();
        var temporaryFilePath = $"{filePath}.{Guid.NewGuid():N}.tmp";
        Directory.CreateDirectory(_rootDirectory);

        try
        {
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(temporaryFilePath, json);
            File.Move(temporaryFilePath, filePath, true);
        }
        finally
        {
            File.Delete(temporaryFilePath);
        }
    }

    public T LoadSettings<T>()
        where T : class, ISettingsGroup, new()
    {
        var filePath = GetFilePath<T>();
        if (!File.Exists(filePath))
        {
            return new T();
        }

        try
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json, JsonOptions) ?? new T();
        }
        catch (JsonException exception)
        {
            LogInvalidSettingsFile(filePath, exception);
            return new T();
        }
    }

    private string GetFilePath<T>() where T : ISettingsGroup
    {
        return Path.Combine(_rootDirectory, $"{T.GroupName}.json");
    }

    [LoggerMessage(LogLevel.Warning, "Settings file {FilePath} contains invalid JSON. Defaults will be used.")]
    private partial void LogInvalidSettingsFile(string filePath, JsonException exception);
}