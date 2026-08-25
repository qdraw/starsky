using Microsoft.Extensions.Logging;
using Velopack;
using Velopack.Sources;

namespace Starsky.Desktop.Services;

public class UpdateService
{
    private const int SuppressMinutes = 5760; // 4 days
    private const string GithubRepoUrl = "https://github.com/qdraw/starsky"; // NOSONAR

    private readonly SettingsService _settings;
    private readonly ILogger<UpdateService> _logger;
    private readonly UpdateManager? _updateManager;
    private UpdateInfo? _pendingUpdate;

    public UpdateService(SettingsService settings, ILogger<UpdateService> logger)
    {
        _settings = settings;
        _logger = logger;

        try
        {
            _updateManager = new UpdateManager(new GithubSource(GithubRepoUrl, null, false));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Velopack not available (running outside installer)");
        }
    }

    public async Task<bool> CheckAsync()
    {
        if (!_settings.Current.UpdateCheckEnabled)
        {
	        return false;
        }

        if (_settings.Current.LastUpdateWarningShown.HasValue)
        {
            var minutesSince = (DateTime.UtcNow - _settings.Current.LastUpdateWarningShown.Value).TotalMinutes;
            if (minutesSince < SuppressMinutes)
            {
	            return false;
            }
        }

        if (_updateManager == null)
        {
	        return false;
        }

        try
        {
            _pendingUpdate = await _updateManager.CheckForUpdatesAsync();
            return _pendingUpdate != null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Update check failed");
            return false;
        }
    }

    public async Task ApplyUpdateAsync()
    {
        if (_updateManager == null || _pendingUpdate == null)
        {
	        throw new InvalidOperationException("No pending update available.");
        }

        await _updateManager.DownloadUpdatesAsync(_pendingUpdate);
        _updateManager.ApplyUpdatesAndRestart(_pendingUpdate);
    }

    public void RecordWarningShown()
    {
        _settings.Current.LastUpdateWarningShown = DateTime.UtcNow;
        _settings.Save();
    }
}
