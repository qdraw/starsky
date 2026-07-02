using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Starsky.Windows.Services;

public sealed class BackendProcessService
{
    private readonly AppPaths _paths;
    private readonly FileLogger _logger;
    private Process? _process;
    private bool _shuttingDown;

    public BackendProcessService(AppPaths paths, FileLogger logger)
    {
        _paths = paths;
        _logger = logger;
    }

    public int Port { get; private set; } = 9609;

    public Uri LocalBaseUri => new($"http://localhost:{Port}");

    public Task StartAsync()
    {
        if (_process is { HasExited: false })
        {
            return Task.CompletedTask;
        }

        Port = GetFreePort();
        var executablePath = _paths.ResolveBackendExecutablePath();
        _logger.Info($"Starting Starsky backend from {executablePath} on port {Port}");

        _process = CreateProcess(executablePath);
        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        return Task.CompletedTask;
    }

    public void Stop()
    {
        _shuttingDown = true;
        if (_process is null)
        {
            return;
        }

        try
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
                _process.WaitForExit(5000);
            }
        }
        catch (Exception exception)
        {
            _logger.Warn($"Stopping backend failed: {exception}");
        }
    }

    private Process CreateProcess(string executablePath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            WorkingDirectory = Path.GetDirectoryName(executablePath)!,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        startInfo.Environment["ASPNETCORE_URLS"] = LocalBaseUri.ToString().TrimEnd('/');
        startInfo.Environment["app__thumbnailTempFolder"] = _paths.ThumbnailTempFolderPath;
        startInfo.Environment["app__tempFolder"] = _paths.BackendTempFolderPath;
        startInfo.Environment["app__appsettingspath"] = _paths.BackendAppSettingsPath;
        startInfo.Environment["app__appsettingslocalpath"] = _paths.BackendAppSettingsLocalPath;
        startInfo.Environment["app__NoAccountLocalhost"] = "true";
        startInfo.Environment["app__UseLocalDesktop"] = "true";
        startInfo.Environment["app__databaseConnection"] = $"Data Source={_paths.BackendDatabasePath}";
        startInfo.Environment["app__ThumbnailGenerationIntervalInMinutes"] = "300";
        startInfo.Environment["app__AccountRegisterDefaultRole"] = "Administrator";
        startInfo.Environment["app__Verbose"] = "false";

        var process = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true,
        };

        process.OutputDataReceived += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.Data))
            {
                _logger.Info(args.Data);
            }
        };

        process.ErrorDataReceived += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.Data))
            {
                _logger.Warn(args.Data);
            }
        };

        process.Exited += async (_, _) =>
        {
            _logger.Warn("Starsky backend exited");
            if (_shuttingDown)
            {
                return;
            }

            await Task.Delay(500);
            try
            {
                await StartAsync();
            }
            catch (Exception exception)
            {
                _logger.Error("Restarting backend failed", exception);
            }
        };

        return process;
    }

    private static int GetFreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}