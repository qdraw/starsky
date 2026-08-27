using Microsoft.Extensions.Logging;
using Starsky.Desktop.Services;

namespace starsky.Tests.Services;

public sealed class DailyFileLoggerProviderTests : IDisposable
{
    private readonly string _logDir;

    public DailyFileLoggerProviderTests()
    {
        _logDir = Path.Combine(Path.GetTempPath(), $"starsky-log-{Guid.NewGuid()}");
        Directory.CreateDirectory(_logDir);
    }

    [Fact]
    public void CreateLogger_ReturnsNonNullLogger()
    {
        using var provider = new DailyFileLoggerProvider(_logDir);
        var logger = provider.CreateLogger("TestCategory");
        Assert.NotNull(logger);
    }

    [Fact]
    public void IsEnabled_InfoAndAbove_ReturnsTrue()
    {
        using var provider = new DailyFileLoggerProvider(_logDir);
        var logger = provider.CreateLogger("Test");
        Assert.True(logger.IsEnabled(LogLevel.Information));
        Assert.True(logger.IsEnabled(LogLevel.Warning));
        Assert.True(logger.IsEnabled(LogLevel.Error));
        Assert.True(logger.IsEnabled(LogLevel.Critical));
    }

    [Fact]
    public void IsEnabled_Debug_ReturnsFalse()
    {
        using var provider = new DailyFileLoggerProvider(_logDir);
        var logger = provider.CreateLogger("Test");
        Assert.False(logger.IsEnabled(LogLevel.Debug));
        Assert.False(logger.IsEnabled(LogLevel.Trace));
    }

    [Fact]
    public void BeginScope_ReturnsNull()
    {
        using var provider = new DailyFileLoggerProvider(_logDir);
        var logger = provider.CreateLogger("Test");
        var scope = logger.BeginScope("state");
        Assert.Null(scope);
    }

    [Fact]
    public void Log_WritesMessageToFile()
    {
        using var provider = new DailyFileLoggerProvider(_logDir);
        var logger = provider.CreateLogger("MyCategory");

        logger.LogInformation("Hello from test");

        var files = Directory.GetFiles(_logDir, "*.log");
        Assert.Single(files);
        var content = File.ReadAllText(files[0]);
        Assert.Contains("Hello from test", content);
        Assert.Contains("MyCategory", content);
    }

    [Fact]
    public void Log_BelowThreshold_DoesNotWrite()
    {
        using var provider = new DailyFileLoggerProvider(_logDir);
        var logger = provider.CreateLogger("Cat");

        logger.LogDebug("should be ignored");

        Assert.Empty(Directory.GetFiles(_logDir, "*.log"));
    }

    [Fact]
    public void Log_WithException_IncludesExceptionInFile()
    {
        using var provider = new DailyFileLoggerProvider(_logDir);
        var logger = provider.CreateLogger("Cat");

        try { throw new InvalidOperationException("test-exception"); }
        catch (Exception ex) { logger.LogError(ex, "an error occurred"); }

        var content = File.ReadAllText(Directory.GetFiles(_logDir, "*.log")[0]);
        Assert.Contains("test-exception", content);
    }

    [Fact]
    public void Log_MultipleEntries_AllAppended()
    {
        using var provider = new DailyFileLoggerProvider(_logDir);
        var logger = provider.CreateLogger("Cat");

        logger.LogInformation("first");
        logger.LogInformation("second");

        var content = File.ReadAllText(Directory.GetFiles(_logDir, "*.log")[0]);
        Assert.Contains("first", content);
        Assert.Contains("second", content);
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var provider = new DailyFileLoggerProvider(_logDir);
        var ex = Record.Exception(() => provider.Dispose());
        Assert.Null(ex);
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var provider = new DailyFileLoggerProvider(_logDir);
        provider.Dispose();
        var ex = Record.Exception(() => provider.Dispose());
        Assert.Null(ex);
    }

    public void Dispose()
    {
        try { Directory.Delete(_logDir, recursive: true); } catch { /* best-effort */ }
        GC.SuppressFinalize(this);
    }
}
