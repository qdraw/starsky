using Microsoft.Extensions.Logging;

namespace Starsky.Desktop.Services;

/// <summary>
/// Encapsulates the startup/shutdown lifecycle of MountWatcher so it can be unit-tested
/// independently of the WPF Application class.
/// </summary>
internal class MountWatcherLifecycle(
    IMountWatcherService mountWatcherService,
    SettingsService settingsService,
    ILogger<MountWatcherLifecycle> logger)
{
    /// <summary>
    /// Called during app startup. Enables the service when the preference is set,
    /// and clears the preference when enable fails so the app does not retry on
    /// every start with a broken binary.
    /// </summary>
    internal async Task OnStartupAsync()
    {
        if (!settingsService.Current.MountWatcherEnabled)
        {
	        return;
        }

        var ok = await mountWatcherService.EnableAsync();
        if (ok)
        {
            logger.LogInformation("MountWatcher enabled on startup");
            return;
        }

        // --install exits non-zero when the service is already registered (sc.exe 1073).
        // Only clear the preference when the service is genuinely absent.
        var status = mountWatcherService.GetStatus();
        if (status is MountWatcherStatus.Running or MountWatcherStatus.Stopped)
        {
            logger.LogInformation("MountWatcher enable failed but service is {Status}; keeping preference", status);
            return;
        }

        logger.LogWarning("MountWatcher enable failed and service is absent; clearing preference");
        settingsService.Current.MountWatcherEnabled = false;
        settingsService.Save();
    }

    /// <summary>
    /// Called during app shutdown (or before a Velopack update restarts the app).
    /// Stops the service synchronously so the binary is not locked when files are replaced.
    /// </summary>
    internal void OnShutdown()
    {
        if (!settingsService.Current.MountWatcherEnabled)
        {
	        return;
        }

        logger.LogInformation("Stopping MountWatcher on shutdown");
        mountWatcherService.StopSync();
    }
}
