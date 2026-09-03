using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Velopack;
using Velopack.Locators;
using Velopack.Sources;

namespace Starsky.Desktop.Services;

public class UpdateService
{
    private const int SuppressMinutes = 5760; // 4 days
    [SuppressMessage("Style", "S1075:URIs should not be hardcoded", Justification = "used")]
    private const string GithubRepoUrl = "https://github.com/qdraw/starsky";

    private readonly SettingsService _settings;
    private readonly ILogger<UpdateService> _logger;
    private UpdateManager? _updateManager;
    private UpdateInfo? _pendingUpdate;

    public UpdateService(SettingsService settings, ILogger<UpdateService> logger)
    {
        _settings = settings;
        _logger = logger;
        IsVelopackAvailable = ProbeInstalled();
    }

    private bool ProbeInstalled()
    {
        try
        {
            var locator = VelopackLocator.CreateDefaultForPlatform();
            return locator?.AppId != null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Velopack not available (running outside installer)");
            return false;
        }
    }

    public async Task<bool> CheckAsync()
    {
        if (!_settings.Current.UpdateCheckEnabled)
        {
            _logger.LogInformation("[UpdateService] Update check is disabled in settings");
            return false;
        }

        if ( !_settings.Current.LastUpdateWarningShown.HasValue )
        {
            _logger.LogInformation("[UpdateService] No previous check recorded; running check now");
            return await CheckWithVelopackAsync();
        }

        var minutesSince = (DateTime.UtcNow - _settings.Current.LastUpdateWarningShown.Value).TotalMinutes;
        _logger.LogInformation("[UpdateService] Minutes since last update warning: {MinutesSince} (suppress threshold: {SuppressMinutes})", minutesSince, SuppressMinutes);

        if (minutesSince < SuppressMinutes)
        {
            _logger.LogInformation("[UpdateService] Suppressing update check (within suppress window)");
            return false;
        }

        _logger.LogInformation("[UpdateService] Suppress window elapsed; running check now");
        return await CheckWithVelopackAsync();
    }

    public Task<bool> CheckNowAsync() => CheckWithVelopackAsync();

    public Task ApplyUpdateAsync()
    {
	    return !HasPendingUpdate ? throw new InvalidOperationException("No pending update available.") : DoApplyUpdateAsync();
    }

    public void RecordWarningShown()
    {
        _settings.Current.LastUpdateWarningShown = DateTime.UtcNow;
        _settings.Save();
    }

    public bool IsVelopackAvailable { get; }

    protected virtual bool HasPendingUpdate => _updateManager != null && _pendingUpdate != null;

    [ExcludeFromCodeCoverage]
    protected virtual async Task<bool> CheckWithVelopackAsync()
    {
        if (!IsVelopackAvailable)
        {
            _logger.LogInformation("[UpdateService] Velopack is not available; skipping update check");
            return false;
        }

        _logger.LogInformation("[UpdateService] Querying GitHub for updates (pre-release: {PreRelease})", _settings.Current.UpdatePreRelease);

        try
        {
            _updateManager = new UpdateManager(
                new GithubSource(GithubRepoUrl, null, _settings.Current.UpdatePreRelease));
            _pendingUpdate = await _updateManager.CheckForUpdatesAsync();

            if (_pendingUpdate != null)
            {
                _logger.LogInformation("[UpdateService] Update available: {Version}", _pendingUpdate.TargetFullRelease.Version);
            }
            else
            {
                _logger.LogInformation("[UpdateService] No update available; already on latest version");
            }

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
