using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Starsky.Desktop.Services;

public class BackendService(ILogger<BackendService> logger) : IDisposable
{
	private Process? _process;
    private int _port;
    private bool _isShuttingDown;
    private bool _hasRestartedOnce;
    private bool _disposed;
	private static readonly string[] SourceArray = ["starsky.exe", "starsky"];

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
            logger.LogError("Backend executable not found in {Dir}", runtimeDir);
            throw new FileNotFoundException($"Starsky backend not found in {runtimeDir}");
        }

        logger.LogInformation("Starting backend: {Exe} on port {Port}", exe, _port);

        var psi = new ProcessStartInfo(exe)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = runtimeDir
        };

        BackendService.SetEnvironment(psi.Environment, _port);

        _process = new Process { StartInfo = psi, EnableRaisingEvents = true };
        _process.OutputDataReceived += (_, e) => { if (e.Data != null)
	        {
		        logger.LogInformation("[backend] {Line}", e.Data);
	        }
        };
        _process.ErrorDataReceived += (_, e) => { if (e.Data != null)
	        {
		        logger.LogWarning("[backend:err] {Line}", e.Data);
	        }
        };
        _process.Exited += OnProcessExited;

        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        await Task.CompletedTask;
    }

    internal static string? FindBackendExe(string dir)
    {
	    return SourceArray.Select(name => Path.Combine(dir, name)).FirstOrDefault(File.Exists);
    }

    internal static void SetEnvironment(IDictionary<string, string?> env, int port)
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
        logger.LogWarning("Backend process exited (shutting down: {IsShuttingDown})", _isShuttingDown);

        if ( _isShuttingDown || _hasRestartedOnce )
        {
	        return;
        }

        _hasRestartedOnce = true;
        logger.LogInformation("Attempting backend restart in 2 s");
        _ = Task.Delay(2000).ContinueWith(async _ =>
        {
	        if (!_isShuttingDown)
	        {
		        await LaunchAsync();
	        }
        }, TaskScheduler.Default);
    }

    public async Task StopAsync()
    {
        _isShuttingDown = true;
        if (_process == null || _process.HasExited)
        {
	        return;
        }

        logger.LogInformation("Stopping backend");
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

        logger.LogInformation("Backend stopped");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
	        return;
        }

        _disposed = true;
        if ( !disposing )
        {
	        return;
        }

        _isShuttingDown = true;
        try { _process?.Kill(); } catch { /* best-effort */ }
        _process?.Dispose();
    }

    internal static async Task<bool> WaitForHealthAsync(
        HttpClient http, string baseUrl,
        Action<string>? onWaiting = null,
        int timeoutSeconds = 60)
    {
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var resp = await http.GetAsync($"{baseUrl}/api/health");
                if (resp.IsSuccessStatusCode)
                {
	                return true;
                }
            }
            catch { /* not yet ready */ }

            onWaiting?.Invoke("Waiting for backend…");
            await Task.Delay(1000);
        }

        throw new TimeoutException($"Backend did not become ready within {timeoutSeconds} seconds.");
    }

    internal static async Task CheckVersionCompatibilityAsync(
        HttpClient http, string baseUrl, string appVersion)
    {
        var resp = await http.PostAsync(
            $"{baseUrl}/api/health/version?version={appVersion}", null);

        if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
	        throw new InvalidOperationException(
		        $"This version ({appVersion}) is incompatible with the server. Please update Starsky Desktop.");
        }
    }
}
