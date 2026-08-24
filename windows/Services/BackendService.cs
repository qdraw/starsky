using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Starsky.Desktop.Services;

public class BackendService : IDisposable
{
    private readonly ILogger<BackendService> _logger;
    private Process? _process;
    private int _port;
    private bool _isShuttingDown;
    private bool _hasRestartedOnce;

    public BackendService(ILogger<BackendService> logger)
    {
        _logger = logger;
    }

    public async Task StartAsync(int port)
    {
        _port = port;
        _isShuttingDown = false;
        _hasRestartedOnce = false;
        await LaunchAsync();
    }

    private async Task LaunchAsync()
    {
        var runtimeDir = ApplicationPaths.RuntimeDir;
        var exe = FindBackendExe(runtimeDir);

        if (exe == null)
        {
            _logger.LogError("Backend executable not found in {Dir}", runtimeDir);
            throw new FileNotFoundException($"Starsky backend not found in {runtimeDir}");
        }

        _logger.LogInformation("Starting backend: {Exe} on port {Port}", exe, _port);

        var psi = new ProcessStartInfo(exe)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = runtimeDir
        };

        SetEnvironment(psi.Environment, _port);

        _process = new Process { StartInfo = psi, EnableRaisingEvents = true };
        _process.OutputDataReceived += (_, e) => { if (e.Data != null)
	        {
		        _logger.LogInformation("[backend] {Line}", e.Data);
	        }
        };
        _process.ErrorDataReceived += (_, e) => { if (e.Data != null)
	        {
		        _logger.LogWarning("[backend:err] {Line}", e.Data);
	        }
        };
        _process.Exited += OnProcessExited;

        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        await Task.CompletedTask;
    }

    private static string? FindBackendExe(string dir)
    {
        foreach (var name in new[] { "starsky.exe", "starsky" })
        {
            var path = Path.Combine(dir, name);
            if (File.Exists(path))
            {
	            return path;
            }
        }
        return null;
    }

    private void SetEnvironment(IDictionary<string, string?> env, int port)
    {
        env["ASPNETCORE_URLS"] = $"http://localhost:{port}";

        // Settings file paths (file paths, not directories — matches Electron)
        env["app__appsettingspath"] = ApplicationPaths.AppSettingsFile;
        env["app__appsettingslocalpath"] = ApplicationPaths.AppSettingsLocalFile;

        // Storage
        env["app__databaseConnection"] = $"Data Source={ApplicationPaths.DatabaseFile}";
        env["app__tempFolder"] = ApplicationPaths.TempFolder;
        env["app__thumbnailTempFolder"] = ApplicationPaths.ThumbnailTempFolder;

        // Desktop-mode flags (exact keys used by Electron)
        env["app__NoAccountLocalhost"] = "true";
        env["app__UseLocalDesktop"] = "true";
        env["app__AccountRegisterDefaultRole"] = "Administrator";

        // Performance / verbosity (packaged-app values, matching Electron's isPackaged=true branch)
        env["app__ThumbnailGenerationIntervalInMinutes"] = "300";
        env["app__Verbose"] = "false";
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        _logger.LogWarning("Backend process exited (shutting down: {IsShuttingDown})", _isShuttingDown);

        if (!_isShuttingDown && !_hasRestartedOnce)
        {
            _hasRestartedOnce = true;
            _logger.LogInformation("Attempting backend restart in 2 s");
            Task.Delay(2000).ContinueWith(_ =>
            {
                if (!_isShuttingDown)
                {
	                _ = LaunchAsync();
                }
            });
        }
    }

    public async Task StopAsync()
    {
        _isShuttingDown = true;
        if (_process == null || _process.HasExited)
        {
	        return;
        }

        _logger.LogInformation("Stopping backend");
        try
        {
            _process.Kill();
            await _process.WaitForExitAsync(CancellationToken.None)
                .WaitAsync(TimeSpan.FromSeconds(5));
        }
        catch
        {
            try { _process.Kill(entireProcessTree: true); } catch { /* best-effort */ }
        }

        _logger.LogInformation("Backend stopped");
    }

    public void Dispose()
    {
        _isShuttingDown = true;
        try { _process?.Kill(); } catch { /* best-effort */ }
        _process?.Dispose();
    }
}
