using System.Diagnostics;
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

    public MountWatcherService(ILogger<MountWatcherService> logger)
        : this(Path.Combine(ApplicationPaths.RuntimeDir, "starskymountwatchercli.exe"), logger) { }

    internal MountWatcherService(string cliBinaryPath, ILogger<MountWatcherService>? logger = null)
    {
        _cliBinaryPath = cliBinaryPath;
        _logger = logger ?? NullLoggerFactory.Instance.CreateLogger<MountWatcherService>();
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
            var psi = new ProcessStartInfo("sc.exe", $"query {ServiceName}")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            if (process == null)
            {
                _logger.LogWarning("[MountWatcher] GetStatus: failed to start sc.exe");
                return MountWatcherStatus.Unknown;
            }

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                _logger.LogDebug("[MountWatcher] GetStatus: service not installed (sc.exe exit {ExitCode})", process.ExitCode);
                return MountWatcherStatus.NotInstalled;
            }

            var status = output.Contains("RUNNING", StringComparison.OrdinalIgnoreCase)
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
            var psi = new ProcessStartInfo(_cliBinaryPath, "--uninstall")
            {
                Verb = "runas",
                UseShellExecute = true,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            process?.WaitForExit();
            _logger.LogInformation("[MountWatcher] StopSync: completed (exit {ExitCode})", process?.ExitCode);
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
                var psi = new ProcessStartInfo(_cliBinaryPath, args)
                {
                    Verb = "runas",
                    UseShellExecute = true,
                    CreateNoWindow = true
                };
                using var process = Process.Start(psi);
                if (process == null)
                {
                    _logger.LogWarning("[MountWatcher] RunElevatedAsync: Process.Start returned null for args={Args}", args);
                    return -1;
                }

                process.WaitForExit();
                _logger.LogDebug("[MountWatcher] RunElevatedAsync: args={Args} exit={ExitCode}", args, process.ExitCode);
                return process.ExitCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[MountWatcher] RunElevatedAsync: failed for args={Args}", args);
                return -1;
            }
        });
    }
}
