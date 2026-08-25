using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Velopack;
using Velopack.Sources;

namespace Starsky.Desktop.Services;

public class UpdateService
{
    private const int SuppressMinutes = 5760; // 4 days
    [SuppressMessage("Style", "S1075:URIs should not be hardcoded", Justification = "used")]
    private const string GithubRepoUrl = "https://github.com/qdraw/starsky";

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

        if ( !_settings.Current.LastUpdateWarningShown.HasValue )
        {
	        return await CheckWithVelopackAsync();
        }

        var minutesSince = (DateTime.UtcNow - _settings.Current.LastUpdateWarningShown.Value).TotalMinutes;
        if (minutesSince < SuppressMinutes)
        {
	        return false;
        }

        return await CheckWithVelopackAsync();
    }

    public Task ApplyUpdateAsync()
    {
	    return !HasPendingUpdate ? throw new InvalidOperationException("No pending update available.") : DoApplyUpdateAsync();
    }

    public void RecordWarningShown()
    {
        _settings.Current.LastUpdateWarningShown = DateTime.UtcNow;
        _settings.Save();
    }

    protected virtual bool HasPendingUpdate => _updateManager != null && _pendingUpdate != null;

    [ExcludeFromCodeCoverage]
    protected virtual async Task<bool> CheckWithVelopackAsync()
    {
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

    [ExcludeFromCodeCoverage]
    protected virtual async Task DoApplyUpdateAsync()
    {
	    if (_updateManager == null)
	    {
		    return;
	    }

        await _updateManager.DownloadUpdatesAsync(_pendingUpdate!);
        _updateManager.ApplyUpdatesAndRestart(_pendingUpdate!);
    }
}
