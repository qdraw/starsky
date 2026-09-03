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
    [SuppressMessage("Style", "S1075:URIs should not be hardcoded", Justification = "used")]
    private const string AppcastBaseUrl = "https://qdraw.nl/special/starsky/appcast-macos/";

    private readonly SettingsService _settings;
    private readonly ILogger<UpdateService> _logger;
    private readonly Func<string, Task<string>> _httpGet;
    private UpdateManager? _updateManager;
    private UpdateInfo? _pendingUpdate;

    public UpdateService(SettingsService settings, ILogger<UpdateService> logger,
        Func<string, Task<string>>? httpGet = null)
    {
        _settings = settings;
        _logger = logger;
        _httpGet = httpGet ?? DefaultHttpGetAsync;
        IsVelopackAvailable = ProbeInstalled();
    }

    [ExcludeFromCodeCoverage]
    private static async Task<string> DefaultHttpGetAsync(string url)
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd("starsky-desktop");
        return await http.GetStringAsync(url);
    }

    private bool ProbeInstalled()
    {
        try
        {
            var locator = VelopackLocator.CreateDefaultForPlatform();
            return locator.AppId != null;
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

        if (!_settings.Current.LastUpdateWarningShown.HasValue)
        {
	        _logger.LogInformation("[UpdateService] No previous check recorded; running check now");
            return await CheckWithVelopackAsync();
        }

        var minutesSince = (DateTime.UtcNow - _settings.Current.LastUpdateWarningShown.Value).TotalMinutes;
        _logger.LogInformation("[UpdateService] Minutes since last update warning: {MinutesSince} (suppress threshold: {SuppressMinutes})", minutesSince, SuppressMinutes);

        if ( !( minutesSince < SuppressMinutes ) )
        {
	        _logger.LogInformation("[UpdateService] Suppress window elapsed; running check now");
	        return await CheckWithVelopackAsync();
        }

        _logger.LogInformation("[UpdateService] Suppressing update check (within suppress window)");
        return false;

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

    /// <summary>Set when the appcast fallback detects a newer release that Velopack cannot install directly.</summary>
    public string? PendingGitHubReleaseUrl { get; private set; }

    /// <summary>True when a newer release was found via the appcast but Velopack has no package for it.</summary>
    public bool IsGitHubFallbackUpdate => PendingGitHubReleaseUrl != null && _pendingUpdate == null;

    protected virtual bool HasPendingUpdate => _updateManager != null && _pendingUpdate != null;

    [ExcludeFromCodeCoverage]
    protected virtual async Task<bool> CheckWithVelopackAsync()
    {
        _logger.LogInformation("[UpdateService] Querying for updates (pre-release: {PreRelease})", _settings.Current.UpdatePreRelease);

        try
        {
            if (IsVelopackAvailable)
            {
                _updateManager = new UpdateManager(
                    new GithubSource(GithubRepoUrl, null, _settings.Current.UpdatePreRelease));
                _pendingUpdate = await _updateManager.CheckForUpdatesAsync();

                if (_pendingUpdate != null)
                {
                    _logger.LogInformation("[UpdateService] Velopack update available: {Version}", _pendingUpdate.TargetFullRelease.Version);
                    return true;
                }

                _logger.LogInformation("[UpdateService] Velopack found no update; trying appcast");
            }
            else
            {
                _logger.LogInformation("[UpdateService] Velopack is not available; trying appcast");
            }

            return await TryAppcastFallbackAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Update check failed");
            return false;
        }
    }

    /// <summary>
    /// Fetches and parses the appcast, sets <see cref="PendingGitHubReleaseUrl"/> when a newer
    /// release is found, and returns whether an update is available. Testable without Velopack.
    /// </summary>
    internal async Task<bool> TryAppcastFallbackAsync()
    {
        var fallback = await CheckAppcastAsync();
        if (fallback.HasValue)
        {
            PendingGitHubReleaseUrl = fallback.Value.HtmlUrl;
            _logger.LogInformation("[UpdateService] Appcast found newer release: {Version}", fallback.Value.Version);
            return true;
        }

        _logger.LogInformation("[UpdateService] No update available; already on latest version");
        return false;
    }

    protected virtual async Task<(string Version, string HtmlUrl)?> CheckAppcastAsync()
    {
        var url = _settings.Current.UpdatePreRelease
            ? AppcastBaseUrl + "?pre-release=1"
            : AppcastBaseUrl;

        var xml = await _httpGet(url);
        return AppcastChecker.FindNewerRelease(xml, ApplicationInfo.Version, _logger);
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
