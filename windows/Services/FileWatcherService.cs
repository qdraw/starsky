namespace Starsky.Windows.Services;

public sealed class FileWatcherService
{
    private readonly FileLogger _logger;
    private FileSystemWatcher? _watcher;

    public FileWatcherService(FileLogger logger)
    {
        _logger = logger;
    }

    public event EventHandler<string>? FileChanged;

    public void Start(string workspacePath)
    {
        Stop();
        Directory.CreateDirectory(workspacePath);

        _watcher = new FileSystemWatcher(workspacePath)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
            EnableRaisingEvents = true,
        };

        _watcher.Changed += HandleChanged;
        _watcher.Created += HandleChanged;
        _watcher.Renamed += (_, args) => HandlePath(args.FullPath);
        _logger.Info($"Watching local workspace {workspacePath}");
    }

    public void Stop()
    {
        if (_watcher is null)
        {
            return;
        }

        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
        _watcher = null;
    }

    private void HandleChanged(object sender, FileSystemEventArgs args)
    {
        HandlePath(args.FullPath);
    }

    private void HandlePath(string fullPath)
    {
        _logger.Info($"Detected file change {fullPath}");
        FileChanged?.Invoke(this, fullPath);
    }
}