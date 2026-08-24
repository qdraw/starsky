using System.Text.Json;
using Microsoft.Extensions.Logging;
using Starsky.Desktop.Models;

namespace Starsky.Desktop.Services;

public class SettingsService
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private readonly ILogger<SettingsService> _logger;
    private readonly string _settingsFile;

    public DesktopSettings Current { get; private set; } = new();

    public SettingsService(ILogger<SettingsService> logger, string? settingsFile = null)
    {
        _logger = logger;
        _settingsFile = settingsFile ?? ApplicationPaths.SettingsFile;
    }

    public DesktopSettings Load()
    {
        if (!File.Exists(_settingsFile))
        {
            _logger.LogInformation("Settings file not found, using defaults");
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
            _logger.LogWarning(ex, "Failed to parse settings, using defaults");
            Current = new DesktopSettings();
        }

        return Current;
    }

    public void Save(DesktopSettings settings)
    {
        try
        {
            Current = settings;
            var json = JsonSerializer.Serialize(settings, _jsonOptions);
            File.WriteAllText(_settingsFile, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings");
        }
    }

    public void Save() => Save(Current);
}
