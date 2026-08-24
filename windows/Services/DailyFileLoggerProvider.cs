using Microsoft.Extensions.Logging;

namespace Starsky.Desktop.Services;

public class DailyFileLoggerProvider : ILoggerProvider
{
    private readonly string _logDir;

    public DailyFileLoggerProvider(string logDir)
    {
        _logDir = logDir;
        Directory.CreateDirectory(logDir);
    }

    public ILogger CreateLogger(string categoryName) => new DailyFileLogger(_logDir, categoryName);

    public void Dispose() { }
}

internal sealed class DailyFileLogger : ILogger
{
    private readonly string _logDir;
    private readonly string _category;
    private static readonly Lock _lock = new();

    public DailyFileLogger(string logDir, string category)
    {
        _logDir = logDir;
        _category = category;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
	        return;
        }

        var msg = formatter(state, exception);
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {_category}: {msg}";
        if (exception != null)
        {
	        line += $"\n{exception}";
        }

        var file = Path.Combine(_logDir, $"starsky-{DateTime.Today:yyyy-MM-dd}.log");
        lock (_lock)
        {
            try { File.AppendAllText(file, line + Environment.NewLine); }
            catch { /* best-effort */ }
        }
    }
}
