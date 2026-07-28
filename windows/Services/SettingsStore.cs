using System.Text.Json;
using System.Text.Json.Serialization;
using Starsky.Windows.Models;

namespace Starsky.Windows.Services;

public sealed class SettingsStore
{
    private readonly AppPaths _paths;
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public SettingsStore(AppPaths paths)
    {
        _paths = paths;
    }

    public async Task<AppSettings> LoadAsync()
    {
        if (!File.Exists(_paths.SettingsFilePath))
        {
            return new AppSettings();
        }

        await using var stream = File.OpenRead(_paths.SettingsFilePath);
        var settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream, _options);
        return settings ?? new AppSettings();
    }

    public async Task SaveAsync(AppSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_paths.SettingsFilePath)!);
        await using var stream = File.Create(_paths.SettingsFilePath);
        await JsonSerializer.SerializeAsync(stream, settings, _options);
    }
}