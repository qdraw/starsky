using System.Text;

namespace Starsky.Windows.Services;

public sealed class FileLogger
{
    private readonly object _sync = new();
    private readonly AppPaths _paths;

    public FileLogger(AppPaths paths)
    {
        _paths = paths;
    }

    public void Info(string message)
    {
        Write("INFO", message);
    }

    public void Warn(string message)
    {
        Write("WARN", message);
    }

    public void Error(string message, Exception? exception = null)
    {
        var fullMessage = exception is null ? message : $"{message}{Environment.NewLine}{exception}";
        Write("ERROR", fullMessage);
    }

    private void Write(string level, string message)
    {
        var line = $"{DateTimeOffset.Now:O} [{level}] {message}";
        Console.WriteLine(line);

        lock (_sync)
        {
            var filePath = Path.Combine(_paths.LogsPath, $"{DateTime.UtcNow:yyyyMMdd}_app_combined.log");
            File.AppendAllText(filePath, line + Environment.NewLine, Encoding.UTF8);
        }
    }
}