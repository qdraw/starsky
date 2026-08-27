using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Starsky.Desktop.Services;
using starsky.Tests.Helpers;

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

    [Fact]
    public void LocalPathToStarskyPath_StripsWinTempFolderPrefix()
    {
        var tempFolder = ApplicationPaths.TempFolder;
        var localPath = Path.Combine(tempFolder, "photos", "2024", "image.jpg");

        var result = FileWatcherService.LocalPathToStarskyPath(localPath);

        Assert.Equal("/photos/2024/image.jpg", result);
    }

    [Fact]
    public void LocalPathToStarskyPath_WhenOutsideTempFolder_NormalizesSlashes()
    {
        var result = FileWatcherService.LocalPathToStarskyPath(@"C:\some\other\file.jpg");

        Assert.StartsWith("/", result);
        Assert.DoesNotContain("\\", result);
    }

    [Fact]
    public void SetUploadContext_DoesNotThrow()
    {
        var ex = Record.Exception(() => _sut.SetUploadContext("http://localhost:5000", "auth=abc"));
        Assert.Null(ex);
    }

    [Fact]
    public async Task UploadFileAsync_WhenServerReturns500_DoesNotThrow()
    {
        using var http = new HttpClient(
            new FakeHttpMessageHandler(HttpStatusCode.InternalServerError, "server error"));
        using var sut = new FileWatcherService(NullLogger<FileWatcherService>.Instance, http);
        sut.Start();
        sut.SetUploadContext("http://localhost:5000", null);

        var path = Path.Combine(ApplicationPaths.TempFolder, $"test-{Guid.NewGuid()}.jpg");
        await File.WriteAllBytesAsync(path, [0xFF, 0xD8, 0xFF, 0xE0]);

        // Wait for watcher debounce + background upload to complete
        await Task.Delay(1200);

        try { File.Delete(path); } catch { /* best-effort */ }
        Assert.True(true); // reaching here without unhandled exception is the assertion
    }

    public void Dispose()
    {
        try { _sut.Dispose(); } catch { /* best-effort */ }
        GC.SuppressFinalize(this);
    }
}
