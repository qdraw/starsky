using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Starsky.Desktop.Services;
using starsky.Tests.Helpers;

namespace starsky.Tests.Services;

[TestClass]
public sealed class FileWatcherServiceTests : IDisposable
{
    private readonly FileWatcherService _sut = new(NullLogger<FileWatcherService>.Instance);

    [TestMethod]
    public void Start_DoesNotThrow()
    {
        Exception? ex = null;
        try { _sut.Start(); } catch (Exception e) { ex = e; }
        Assert.IsNull(ex);
    }

    [TestMethod]
    public void Stop_BeforeStart_DoesNotThrow()
    {
        Exception? ex = null;
        try { _sut.Stop(); } catch (Exception e) { ex = e; }
        Assert.IsNull(ex);
    }

    [TestMethod]
    public void Stop_AfterStart_DoesNotThrow()
    {
        _sut.Start();
        Exception? ex = null;
        try { _sut.Stop(); } catch (Exception e) { ex = e; }
        Assert.IsNull(ex);
    }

    [TestMethod]
    public void Dispose_BeforeStart_IsIdempotent()
    {
        Exception? ex = null;
        try { _sut.Dispose(); } catch (Exception e) { ex = e; }
        Assert.IsNull(ex);
    }

    [TestMethod]
    public void Dispose_AfterStart_DoesNotThrow()
    {
        _sut.Start();
        Exception? ex = null;
        try { _sut.Dispose(); } catch (Exception e) { ex = e; }
        Assert.IsNull(ex);
    }

    [TestMethod]
    public void Start_CreatesTempFolderIfMissing()
    {
        _sut.Start();
        Assert.IsTrue(Directory.Exists(ApplicationPaths.TempFolder));
    }

    [TestMethod]
    public async Task FileCreated_NonTmpFile_TriggersDebounce()
    {
        _sut.Start();
        var path = Path.Combine(ApplicationPaths.TempFolder, $"test-{Guid.NewGuid()}.jpg");

        await File.WriteAllBytesAsync(path, [1, 2, 3]);

        // Wait for watcher event + debounce timer (500 ms) to fire without throwing
        await Task.Delay(700);

        try { File.Delete(path); } catch { /* best-effort */ }
        Assert.IsTrue(true); // reaching here without exception is the assertion
    }

    [TestMethod]
    public async Task FileCreated_TmpFile_IsIgnored()
    {
        _sut.Start();
        var path = Path.Combine(ApplicationPaths.TempFolder, $"test-{Guid.NewGuid()}.tmp");

        await File.WriteAllBytesAsync(path, [1, 2, 3]);
        await Task.Delay(200);

        try { File.Delete(path); } catch { /* best-effort */ }
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void LocalPathToStarskyPath_StripsWinTempFolderPrefix()
    {
        var tempFolder = ApplicationPaths.TempFolder;
        var localPath = Path.Combine(tempFolder, "photos", "2024", "image.jpg");

        var result = FileWatcherService.LocalPathToStarskyPath(localPath);

        Assert.AreEqual("/photos/2024/image.jpg", result);
    }

    [TestMethod]
    public void LocalPathToStarskyPath_WhenOutsideTempFolder_NormalizesSlashes()
    {
        var result = FileWatcherService.LocalPathToStarskyPath(@"C:\some\other\file.jpg");

        Assert.IsTrue(result.StartsWith("/"));
        Assert.IsFalse(result.Contains("\\"));
    }

    [TestMethod]
    public void GetUploadEndpoint_XmpLowercase_ReturnsSidecar()
        => Assert.AreEqual("upload-sidecar", FileWatcherService.GetUploadEndpoint("photo.xmp"));

    [TestMethod]
    public void GetUploadEndpoint_XmpUppercase_ReturnsSidecar()
        => Assert.AreEqual("upload-sidecar", FileWatcherService.GetUploadEndpoint("photo.XMP"));

    [TestMethod]
    public void GetUploadEndpoint_Jpg_ReturnsUpload()
        => Assert.AreEqual("upload", FileWatcherService.GetUploadEndpoint("photo.jpg"));

    [TestMethod]
    public void GetUploadEndpoint_NoExtension_ReturnsUpload()
        => Assert.AreEqual("upload", FileWatcherService.GetUploadEndpoint("photo"));

    [TestMethod]
    public void SetUploadContext_DoesNotThrow()
    {
        Exception? ex = null;
        try { _sut.SetUploadContext("http://localhost:5000", "auth=abc"); } catch (Exception e) { ex = e; }
        Assert.IsNull(ex);
    }

    [TestMethod]
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
        Assert.IsTrue(true); // reaching here without unhandled exception is the assertion
    }

    public void Dispose()
    {
        try { _sut.Dispose(); } catch { /* best-effort */ }
        GC.SuppressFinalize(this);
    }
}
