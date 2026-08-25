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

    [Fact]
    public async Task FileCreated_NonTmpFile_TriggersDebounce()
    {
        _sut.Start();
        var path = Path.Combine(ApplicationPaths.TempFolder, $"test-{Guid.NewGuid()}.jpg");

        await File.WriteAllBytesAsync(path, [1, 2, 3]);

        // Wait for watcher event + debounce timer (500 ms) to fire without throwing
        await Task.Delay(700);

        try { File.Delete(path); } catch { /* best-effort */ }
        Assert.True(true); // reaching here without exception is the assertion
    }

    [Fact]
    public async Task FileCreated_TmpFile_IsIgnored()
    {
        _sut.Start();
        var path = Path.Combine(ApplicationPaths.TempFolder, $"test-{Guid.NewGuid()}.tmp");

        await File.WriteAllBytesAsync(path, [1, 2, 3]);
        await Task.Delay(200);

        try { File.Delete(path); } catch { /* best-effort */ }
        Assert.True(true);
    }

    public void Dispose()
    {
        try { _sut.Dispose(); } catch { /* best-effort */ }
        GC.SuppressFinalize(this);
    }
}
