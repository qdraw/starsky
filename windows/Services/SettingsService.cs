using System.Text.Json;
using Microsoft.Extensions.Logging;
using Starsky.Desktop.Models;

namespace Starsky.Desktop.Services;

public class SettingsService(ILogger<SettingsService> logger, string? settingsFile = null)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _settingsFile = settingsFile ?? ApplicationPaths.SettingsFile;

    public DesktopSettings Current { get; private set; } = new();

    public DesktopSettings Load()
    {
        if (!File.Exists(_settingsFile))
        {
            logger.LogInformation("Settings file not found, using defaults");
            Current = new DesktopSettings();
            return Current;
        }

        try
        {
            var json = File.ReadAllText(_settingsFile);
            Current = JsonSerializer.Deserialize<DesktopSettings>(json) ?? new DesktopSettings();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to parse settings, using defaults");
            Current = new DesktopSettings();
        }

        return Current;
    }

    public void Save(DesktopSettings settings)
    {
        try
        {
            Current = settings;
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(_settingsFile, json);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save settings");
        }
    }

    public void Save() => Save(Current);
}
