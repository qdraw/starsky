using Microsoft.Extensions.Logging;
using Starsky.Desktop.Services;

namespace starsky.Tests.Services;

[TestClass]
public sealed class DailyFileLoggerProviderTests : IDisposable
{
    private readonly string _logDir;

    public DailyFileLoggerProviderTests()
    {
        _logDir = Path.Combine(Path.GetTempPath(), $"starsky-log-{Guid.NewGuid()}");
        Directory.CreateDirectory(_logDir);
    }

    [TestMethod]
    public void CreateLogger_ReturnsNonNullLogger()
    {
        using var provider = new DailyFileLoggerProvider(_logDir);
        var logger = provider.CreateLogger("TestCategory");
        Assert.IsNotNull(logger);
    }

    [TestMethod]
    public void IsEnabled_InfoAndAbove_ReturnsTrue()
    {
        using var provider = new DailyFileLoggerProvider(_logDir);
        var logger = provider.CreateLogger("Test");
        Assert.IsTrue(logger.IsEnabled(LogLevel.Information));
        Assert.IsTrue(logger.IsEnabled(LogLevel.Warning));
        Assert.IsTrue(logger.IsEnabled(LogLevel.Error));
        Assert.IsTrue(logger.IsEnabled(LogLevel.Critical));
    }

    [TestMethod]
    public void IsEnabled_Debug_ReturnsFalse()
    {
        using var provider = new DailyFileLoggerProvider(_logDir);
        var logger = provider.CreateLogger("Test");
        Assert.IsFalse(logger.IsEnabled(LogLevel.Debug));
        Assert.IsFalse(logger.IsEnabled(LogLevel.Trace));
    }

    [TestMethod]
    public void BeginScope_ReturnsNull()
    {
        using var provider = new DailyFileLoggerProvider(_logDir);
        var logger = provider.CreateLogger("Test");
        var scope = logger.BeginScope("state");
        Assert.IsNull(scope);
    }

    [TestMethod]
    public void Log_WritesMessageToFile()
    {
        using var provider = new DailyFileLoggerProvider(_logDir);
        var logger = provider.CreateLogger("MyCategory");

        logger.LogInformation("Hello from test");

        var files = Directory.GetFiles(_logDir, "*.log");
        Assert.AreEqual(1, files.Length);
        var content = File.ReadAllText(files[0]);
        Assert.IsTrue(content.Contains("Hello from test"));
        Assert.IsTrue(content.Contains("MyCategory"));
    }

    [TestMethod]
    public void Log_BelowThreshold_DoesNotWrite()
    {
        using var provider = new DailyFileLoggerProvider(_logDir);
        var logger = provider.CreateLogger("Cat");

        logger.LogDebug("should be ignored");

        Assert.AreEqual(0, Directory.GetFiles(_logDir, "*.log").Length);
    }

    [TestMethod]
    public void Log_WithException_IncludesExceptionInFile()
    {
        using var provider = new DailyFileLoggerProvider(_logDir);
        var logger = provider.CreateLogger("Cat");

        try { throw new InvalidOperationException("test-exception"); }
        catch (Exception ex) { logger.LogError(ex, "an error occurred"); }

        var content = File.ReadAllText(Directory.GetFiles(_logDir, "*.log")[0]);
        Assert.IsTrue(content.Contains("test-exception"));
    }

    [TestMethod]
    public void Log_MultipleEntries_AllAppended()
    {
        using var provider = new DailyFileLoggerProvider(_logDir);
        var logger = provider.CreateLogger("Cat");

        logger.LogInformation("first");
        logger.LogInformation("second");

        var content = File.ReadAllText(Directory.GetFiles(_logDir, "*.log")[0]);
        Assert.IsTrue(content.Contains("first"));
        Assert.IsTrue(content.Contains("second"));
    }

    [TestMethod]
    public void Dispose_DoesNotThrow()
    {
        var provider = new DailyFileLoggerProvider(_logDir);
        Exception? ex = null;
        try { provider.Dispose(); } catch (Exception e) { ex = e; }
        Assert.IsNull(ex);
    }

    [TestMethod]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var provider = new DailyFileLoggerProvider(_logDir);
        provider.Dispose();
        Exception? ex = null;
        try { provider.Dispose(); } catch (Exception e) { ex = e; }
        Assert.IsNull(ex);
    }

    public void Dispose()
    {
        try { Directory.Delete(_logDir, recursive: true); } catch { /* best-effort */ }
        GC.SuppressFinalize(this);
    }
}
