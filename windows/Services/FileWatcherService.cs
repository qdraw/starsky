using System.IO;
using Microsoft.Extensions.Logging;

namespace Starsky.Desktop.Services;

public class FileWatcherService(ILogger<FileWatcherService> logger) : IDisposable
{
	private FileSystemWatcher? _watcher;
    private readonly Dictionary<string, System.Timers.Timer> _debounceTimers = new();
    private readonly Lock _lock = new();
    private bool _disposed;

    public void Start()
    {
        Stop();

        var path = ApplicationPaths.TempFolder;
        Directory.CreateDirectory(path);

        _watcher = new FileSystemWatcher(path)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
            EnableRaisingEvents = true
        };

        _watcher.Created += OnChanged;
        _watcher.Changed += OnChanged;
        logger.LogInformation("File watcher started on {Path}", path);
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        // Skip .tmp files still being written
        if (Path.GetExtension(e.FullPath).Equals(".tmp", StringComparison.OrdinalIgnoreCase))
        {
	        return;
        }

        lock (_lock)
        {
            if (_debounceTimers.TryGetValue(e.FullPath, out var existing))
            {
                existing.Stop();
                existing.Dispose();
            }

            var timer = new System.Timers.Timer(500) { AutoReset = false };
            timer.Elapsed += (_, _) => HandleFileChanged(e.FullPath);
            _debounceTimers[e.FullPath] = timer;
            timer.Start();
        }
    }

    private void HandleFileChanged(string path)
    {
        lock (_lock)
        {
	        _debounceTimers.Remove(path);
        }

        logger.LogInformation("File changed in workspace: {Path}", path);
    }

    public void Stop()
    {
        _watcher?.Dispose();
        _watcher = null;
        lock (_lock)
        {
            foreach (var t in _debounceTimers.Values)
            {
	            t.Dispose();
            }

            _debounceTimers.Clear();
        }
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
        if (disposing)
        {
	        Stop();
        }
    }
}
