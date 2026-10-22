using Microsoft.Extensions.Logging;

namespace Starsky.Desktop.Services;

public class DailyFileLoggerProvider : ILoggerProvider
{
    private readonly string _logDir;
    private bool _disposed;

    public DailyFileLoggerProvider(string logDir)
    {
        _logDir = logDir;
        Directory.CreateDirectory(logDir);
    }

    public ILogger CreateLogger(string categoryName) => new DailyFileLogger(_logDir, categoryName);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool _)
    {
        if (_disposed)
        {
	        return;
        }

        _disposed = true;
    }
}

internal sealed class DailyFileLogger(string logDir, string category) : ILogger
{
	private static readonly Lock Lock = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
	        return;
        }

        var msg = formatter(state, exception);
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {category}: {msg}";
        if (exception != null)
        {
	        line += $"\n{exception}";
        }

        var file = Path.Combine(logDir, $"starsky-{DateTime.Today:yyyy-MM-dd}.log");
        lock (Lock)
        {
            try { File.AppendAllText(file, line + Environment.NewLine); }
            catch { /* best-effort */ }
        }
    }
}
