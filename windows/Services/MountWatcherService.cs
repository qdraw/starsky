using System.Diagnostics;

namespace Starsky.Desktop.Services;

public class MountWatcherService : IMountWatcherService
{
#if DEBUG
    internal const string ServiceName = "starsky-mountwatcher-debug";
#else
    internal const string ServiceName = "starsky-mountwatcher";
#endif

    private readonly string _cliBinaryPath;

    public MountWatcherService()
        : this(Path.Combine(ApplicationPaths.RuntimeDir, "starskymountwatchercli.exe")) { }

    internal MountWatcherService(string cliBinaryPath)
    {
        _cliBinaryPath = cliBinaryPath;
    }

    public async Task<bool> EnableAsync()
    {
        if (!File.Exists(_cliBinaryPath))
            return false;

        var exitCode = await RunElevatedAsync("--install");
        return exitCode == 0;
    }

    public async Task<bool> DisableAsync()
    {
        if (!File.Exists(_cliBinaryPath))
            return GetStatus() == MountWatcherStatus.NotInstalled;

        var exitCode = await RunElevatedAsync("--uninstall");
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
            if (process == null) return MountWatcherStatus.Unknown;
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0) return MountWatcherStatus.NotInstalled;

            return output.Contains("RUNNING", StringComparison.OrdinalIgnoreCase)
                ? MountWatcherStatus.Running
                : MountWatcherStatus.Stopped;
        }
        catch
        {
            return MountWatcherStatus.Unknown;
        }
    }

    public void StopSync()
    {
        if (!File.Exists(_cliBinaryPath))
            return;

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
        }
        catch
        {
            // fire-and-forget; app is shutting down
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
                if (process == null) return -1;
                process.WaitForExit();
                return process.ExitCode;
            }
            catch
            {
                return -1;
            }
        });
    }
}
