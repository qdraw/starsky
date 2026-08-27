using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace Starsky.Desktop.Services;

public class FileWatcherService(ILogger<FileWatcherService> logger, HttpClient? http = null) : IDisposable
{
    private readonly HttpClient _http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
    private FileSystemWatcher? _watcher;
    private readonly Dictionary<string, System.Timers.Timer> _debounceTimers = new();
    private readonly Lock _lock = new();
    private bool _disposed;
    private string? _uploadBaseUrl;
    private string? _uploadCookieHeader;

    public void SetUploadContext(string baseUrl, string? cookieHeader)
    {
        _uploadBaseUrl = baseUrl;
        _uploadCookieHeader = cookieHeader;
    }

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
        _watcher.Renamed += OnRenamed;
        logger.LogInformation("File watcher started on {Path}", path);
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
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

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        // Skip the .tmp → final rename produced by FileDownloadService — that is the
        // initial download, not a user edit.  Any other rename (editor temp-file pattern)
        // is treated as a real change on the new path.
        if (Path.GetExtension(e.OldFullPath).Equals(".tmp", StringComparison.OrdinalIgnoreCase))
        {
	        return;
        }

        OnChanged(sender, e);
    }

    private void HandleFileChanged(string path)
    {
        lock (_lock)
        {
            _debounceTimers.Remove(path);
        }

        // Directories fire Changed when a child is written — skip them.
        if (!File.Exists(path))
        {
	        return;
        }

        logger.LogInformation("File changed in workspace: {Path}", path);

        if (_uploadBaseUrl != null)
        {
	        _ = Task.Run(() => UploadFileAsync(path));
        }
    }

    private async Task UploadFileAsync(string localPath)
    {
        var starskyPath = LocalPathToStarskyPath(localPath);
        logger.LogInformation("Uploading {LocalPath} → {ServerPath}", localPath, starskyPath);

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post,
                $"{_uploadBaseUrl!.TrimEnd('/')}/starsky/api/upload");
            req.Headers.TryAddWithoutValidation("to", starskyPath);
            if (_uploadCookieHeader != null)
            {
	            req.Headers.TryAddWithoutValidation("Cookie", _uploadCookieHeader);
            }

            await using var fileStream = File.OpenRead(localPath);
            req.Content = new StreamContent(fileStream);
            req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                logger.LogError("Upload failed ({Status}) for {Path}: {Body}",
                    (int)resp.StatusCode, starskyPath, body);
                return;
            }

            logger.LogInformation("Upload complete: {ServerPath}", starskyPath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Upload failed for {Path}", starskyPath);
        }
    }

    internal static string LocalPathToStarskyPath(string localPath)
    {
        var tempFolder = ApplicationPaths.TempFolder;
        var relative = localPath.StartsWith(tempFolder, StringComparison.OrdinalIgnoreCase)
            ? localPath[tempFolder.Length..]
            : localPath;
        return "/" + relative.Replace('\\', '/').TrimStart('/');
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
