using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Starsky.Desktop.Services;

public class MountWatcherService : IMountWatcherService
{
    // The CLI binary in the runtime directory is always a Release build,
    // so it always registers the service under the production name.
    internal const string ServiceName = "starsky-mountwatcher";

    private readonly string _cliBinaryPath;
    private readonly ILogger<MountWatcherService> _logger;
    private readonly IProcessRunner _runner;

    public MountWatcherService(ILogger<MountWatcherService> logger)
        : this(Path.Combine(ApplicationPaths.RuntimeDir, "starskymountwatchercli.exe"),
               logger,
               new WindowsProcessRunner()) { }

    internal MountWatcherService(
        string cliBinaryPath,
        ILogger<MountWatcherService>? logger = null,
        IProcessRunner? runner = null)
    {
        _cliBinaryPath = cliBinaryPath;
        _logger = logger ?? NullLoggerFactory.Instance.CreateLogger<MountWatcherService>();
        _runner = runner ?? new WindowsProcessRunner();
    }

    public async Task<bool> EnableAsync()
    {
        _logger.LogInformation("[MountWatcher] EnableAsync: binary={Path}", _cliBinaryPath);

        if (!File.Exists(_cliBinaryPath))
        {
            _logger.LogWarning("[MountWatcher] EnableAsync: binary not found at {Path}", _cliBinaryPath);
            return false;
        }

        var exitCode = await RunElevatedAsync("--install");
        if (exitCode == 0)
        {
            _logger.LogInformation("[MountWatcher] EnableAsync: install succeeded");
        }
        else
        {
            _logger.LogWarning("[MountWatcher] EnableAsync: install failed with exit code {ExitCode}", exitCode);
        }
        return exitCode == 0;
    }

    public async Task<bool> DisableAsync()
    {
        _logger.LogInformation("[MountWatcher] DisableAsync: binary={Path}", _cliBinaryPath);

        if (!File.Exists(_cliBinaryPath))
        {
            var status = GetStatus();
            _logger.LogWarning("[MountWatcher] DisableAsync: binary not found; current status={Status}", status);
            return status == MountWatcherStatus.NotInstalled;
        }

        var exitCode = await RunElevatedAsync("--uninstall");
        if (exitCode == 0)
        {
            _logger.LogInformation("[MountWatcher] DisableAsync: uninstall succeeded");
        }
        else
        {
            _logger.LogWarning("[MountWatcher] DisableAsync: uninstall failed with exit code {ExitCode}", exitCode);
        }
        return exitCode == 0;
    }

    public MountWatcherStatus GetStatus()
    {
        try
        {
            var result = _runner.Run(
                Path.Combine(Environment.SystemDirectory, "sc.exe"),
                $"query {ServiceName}");

            if (result.ExitCode != 0)
            {
                _logger.LogDebug("[MountWatcher] GetStatus: service not installed (sc.exe exit {ExitCode})", result.ExitCode);
                return MountWatcherStatus.NotInstalled;
            }

            var status = result.Output.Contains("RUNNING", StringComparison.OrdinalIgnoreCase)
                ? MountWatcherStatus.Running
                : MountWatcherStatus.Stopped;

            _logger.LogDebug("[MountWatcher] GetStatus: {Status}", status);
            return status;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[MountWatcher] GetStatus: unexpected error");
            return MountWatcherStatus.Unknown;
        }
    }

    public void StopSync()
    {
        _logger.LogInformation("[MountWatcher] StopSync: stopping service synchronously");

        if (!File.Exists(_cliBinaryPath))
        {
            _logger.LogWarning("[MountWatcher] StopSync: binary not found at {Path}; skipping", _cliBinaryPath);
            return;
        }

        try
        {
            var result = _runner.RunElevated(_cliBinaryPath, "--uninstall");
            _logger.LogInformation("[MountWatcher] StopSync: completed (exit {ExitCode})", result.ExitCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[MountWatcher] StopSync: failed (app is shutting down)");
        }
    }

    private Task<int> RunElevatedAsync(string args)
    {
        return Task.Run(() =>
        {
            try
            {
                var result = _runner.RunElevated(_cliBinaryPath, args);
                _logger.LogDebug("[MountWatcher] RunElevatedAsync: args={Args} exit={ExitCode}", args, result.ExitCode);
                return result.ExitCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[MountWatcher] RunElevatedAsync: failed for args={Args}", args);
                return -1;
            }
        });
    }
}
