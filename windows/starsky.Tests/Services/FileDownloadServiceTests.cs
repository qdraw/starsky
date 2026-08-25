using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Starsky.Desktop.Services;
using starsky.Tests.Helpers;

namespace starsky.Tests.Services;

public sealed class FileDownloadServiceTests : IDisposable
{
    private const string StarskyPath = "/test-session/photo.jpg";
    private readonly string _expectedFile;

    public FileDownloadServiceTests()
    {
        // Replicate the service's path calculation exactly so the assertion matches on Windows too.
        var parentDir = (Path.GetDirectoryName(StarskyPath)?.TrimStart('/') ?? string.Empty)
                        .Replace('/', Path.DirectorySeparatorChar);
        _expectedFile = Path.Combine(ApplicationPaths.TempFolder, parentDir, Path.GetFileName(StarskyPath));
    }

    private static FileDownloadService Create(params HttpResponseMessage[] responses) =>
        new FileDownloadService(
            NullLogger<FileDownloadService>.Instance,
            new HttpClient(new FakeHttpMessageHandler(responses)));

    // Subclass that records the file path passed to OpenWithDefaultApp instead of opening it
    private sealed class TrackingFileDownloadService(HttpClient http)
        : FileDownloadService(NullLogger<FileDownloadService>.Instance, http)
    {
        public string? OpenedFile { get; private set; }
        protected override void OpenWithDefaultApp(string filePath) => OpenedFile = filePath;
    }

    private static TrackingFileDownloadService CreateTracking(params HttpResponseMessage[] responses) =>
        new TrackingFileDownloadService(new HttpClient(new FakeHttpMessageHandler(responses)));

    [Fact]
    public async Task DownloadAndOpenAsync_ValidPath_WritesFileToDisk()
    {
        var photoBytes = new byte[] { 0xFF, 0xD8, 0xFF }; // minimal JPEG header
        var svc = Create(
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") },
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Array.Empty<byte>()) },
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(photoBytes) });

        await svc.DownloadAndOpenAsync(StarskyPath, "http://localhost:5000", openFile: false);

        Assert.True(File.Exists(_expectedFile));
        Assert.Equal(photoBytes, await File.ReadAllBytesAsync(_expectedFile));
    }

    [Fact]
    public async Task DownloadAndOpenAsync_SidecarFails_StillDownloadsMain()
    {
        var photoBytes = new byte[] { 1, 2, 3 };
        var svc = Create(
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") },
            new HttpResponseMessage(HttpStatusCode.NotFound),
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(photoBytes) });

        await svc.DownloadAndOpenAsync(StarskyPath, "http://localhost:5000", openFile: false);

        Assert.True(File.Exists(_expectedFile));
    }

    [Fact]
    public async Task DownloadAndOpenAsync_PhotoDownloadFails_Throws()
    {
        var svc = Create(
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") },
            new HttpResponseMessage(HttpStatusCode.NotFound),
            new HttpResponseMessage(HttpStatusCode.InternalServerError));

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            svc.DownloadAndOpenAsync(StarskyPath, "http://localhost:5000", openFile: false));
    }

    [Fact]
    public async Task DownloadAndOpenAsync_SidecarReturnsEmptyBytes_SkipsXmpFile()
    {
        var photoBytes = new byte[] { 0xFF, 0xD8, 0xFF };
        var svc = Create(
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") },
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Array.Empty<byte>()) }, // empty sidecar
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(photoBytes) });

        await svc.DownloadAndOpenAsync(StarskyPath, "http://localhost:5000", openFile: false);

        Assert.True(File.Exists(_expectedFile));
        var localDir = Path.GetDirectoryName(_expectedFile)!;
        Assert.DoesNotContain(Directory.GetFiles(localDir), f => f.EndsWith(".xmp", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task DownloadAndOpenAsync_WhenOpenFileTrue_CallsOpenWithDefaultApp()
    {
        var photoBytes = new byte[] { 0xFF, 0xD8, 0xFF };
        var svc = CreateTracking(
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") },
            new HttpResponseMessage(HttpStatusCode.NotFound),
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(photoBytes) });

        await svc.DownloadAndOpenAsync(StarskyPath, "http://localhost:5000", openFile: true);

        Assert.NotNull(svc.OpenedFile);
        Assert.EndsWith("photo.jpg", svc.OpenedFile, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DownloadAndOpenAsync_RootLevelPath_UsesBaseOfTempFolder()
    {
        const string rootPath = "root-photo.jpg";
        var photoBytes = new byte[] { 1, 2, 3 };
        var svc = Create(
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") },
            new HttpResponseMessage(HttpStatusCode.NotFound),
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(photoBytes) });

        await svc.DownloadAndOpenAsync(rootPath, "http://localhost:5000", openFile: false);

        var expectedFile = Path.Combine(ApplicationPaths.TempFolder, rootPath);
        Assert.True(File.Exists(expectedFile));
        try { File.Delete(expectedFile); } catch { /* best-effort */ }
    }

    public void Dispose()
    {
        try { File.Delete(_expectedFile); } catch { /* best-effort */ }
        GC.SuppressFinalize(this);
    }
}
