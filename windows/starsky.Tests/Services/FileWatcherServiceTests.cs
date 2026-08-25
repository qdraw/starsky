using Microsoft.Extensions.Logging.Abstractions;
using Starsky.Desktop.Services;

namespace starsky.Tests.Services;

public sealed class FileWatcherServiceTests : IDisposable
{
    private readonly FileWatcherService _sut = new(NullLogger<FileWatcherService>.Instance);

    [Fact]
    public void Start_DoesNotThrow()
    {
        var ex = Record.Exception(() => _sut.Start());
        Assert.Null(ex);
    }

    [Fact]
    public void Stop_BeforeStart_DoesNotThrow()
    {
        var ex = Record.Exception(() => _sut.Stop());
        Assert.Null(ex);
    }

    [Fact]
    public void Stop_AfterStart_DoesNotThrow()
    {
        _sut.Start();
        var ex = Record.Exception(() => _sut.Stop());
        Assert.Null(ex);
    }

    [Fact]
    public void Dispose_BeforeStart_IsIdempotent()
    {
        var ex = Record.Exception(() => _sut.Dispose());
        Assert.Null(ex);
    }

    [Fact]
    public void Dispose_AfterStart_DoesNotThrow()
    {
        _sut.Start();
        var ex = Record.Exception(() => _sut.Dispose());
        Assert.Null(ex);
    }

    [Fact]
    public void Start_CreatesTempFolderIfMissing()
    {
        _sut.Start();
        Assert.True(Directory.Exists(ApplicationPaths.TempFolder));
    }

    public void Dispose()
    {
        try { _sut.Dispose(); } catch { /* best-effort */ }
        GC.SuppressFinalize(this);
    }
}
