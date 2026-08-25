using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Starsky.Desktop.Services;
using starsky.Tests.Helpers;

namespace starsky.Tests.Services;

public class BackendServiceTests
{
    [Fact]
    public async Task StopAsync_WhenNotStarted_DoesNotThrow()
    {
        var svc = new BackendService(NullLogger<BackendService>.Instance);

        var ex = await Record.ExceptionAsync(() => svc.StopAsync());

        Assert.Null(ex);
    }

    [Fact]
    public void Dispose_WhenNotStarted_DoesNotThrow()
    {
        var svc = new BackendService(NullLogger<BackendService>.Instance);

        var ex = Record.Exception(() => svc.Dispose());

        Assert.Null(ex);
    }

    [Fact]
    public void SetEnvironment_SetsAllRequiredKeys()
    {
        var env = new Dictionary<string, string?>();

        BackendService.SetEnvironment(env, 12345);

        Assert.Equal("http://localhost:12345", env["ASPNETCORE_URLS"]);
        Assert.Equal("true", env["app__NoAccountLocalhost"]);
        Assert.Equal("true", env["app__UseLocalDesktop"]);
        Assert.Equal("Administrator", env["app__AccountRegisterDefaultRole"]);
        Assert.Equal("false", env["app__Verbose"]);
        Assert.Contains("app__databaseConnection", env.Keys);
        Assert.Contains("app__tempFolder", env.Keys);
        Assert.Contains("app__appsettingspath", env.Keys);
    }

    [Fact]
    public void FindBackendExe_WhenNoExeExists_ReturnsNull()
    {
        var emptyDir = Path.Combine(Path.GetTempPath(), $"starsky-empty-{Guid.NewGuid()}");
        Directory.CreateDirectory(emptyDir);

        try
        {
            var result = BackendService.FindBackendExe(emptyDir);

            Assert.Null(result);
        }
        finally
        {
            Directory.Delete(emptyDir);
        }
    }

    [Fact]
    public void FindBackendExe_WhenExeExists_ReturnsPath()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"starsky-exe-{Guid.NewGuid()}");
        Directory.CreateDirectory(dir);
        var exePath = Path.Combine(dir, "starsky.exe");
        File.WriteAllBytes(exePath, Array.Empty<byte>());

        try
        {
            var result = BackendService.FindBackendExe(dir);

            Assert.Equal(exePath, result);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void FindBackendExe_WhenLinuxExeExists_ReturnsPath()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"starsky-linux-{Guid.NewGuid()}");
        Directory.CreateDirectory(dir);
        var exePath = Path.Combine(dir, "starsky");
        File.WriteAllBytes(exePath, Array.Empty<byte>());

        try
        {
            var result = BackendService.FindBackendExe(dir);

            Assert.Equal(exePath, result);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var svc = new BackendService(NullLogger<BackendService>.Instance);
        svc.Dispose();

        var ex = Record.Exception(() => svc.Dispose());

        Assert.Null(ex);
    }

    [Fact]
    public void SetEnvironment_ContainsThumbnailAndSettingsPaths()
    {
        var env = new Dictionary<string, string?>();

        BackendService.SetEnvironment(env, 5000);

        Assert.Contains("app__thumbnailTempFolder", env.Keys);
        Assert.Contains("app__appsettingslocalpath", env.Keys);
        Assert.Equal("300", env["app__ThumbnailGenerationIntervalInMinutes"]);
    }

    [Fact]
    public async Task StartAsync_WhenExeNotFound_Throws()
    {
        // In the test environment ApplicationPaths.RuntimeDir does not contain a starsky.exe,
        // so LaunchAsync cannot find the backend and throws FileNotFoundException.
        if (BackendService.FindBackendExe(ApplicationPaths.RuntimeDir) != null)
        {
	        return; // Runtime present in test env — skip
        }

        var svc = new BackendService(NullLogger<BackendService>.Instance);

        await Assert.ThrowsAsync<FileNotFoundException>(() => svc.StartAsync(5000));
    }

    // ── WaitForHealthAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task WaitForHealthAsync_WhenServerReturns200_Returns()
    {
        using var http = new HttpClient(new FakeHttpMessageHandler(HttpStatusCode.OK));

        var result = await BackendService.WaitForHealthAsync(http, "http://localhost:5000");
        
        Assert.True(result);
    }

    [Fact]
    public async Task WaitForHealthAsync_WhenServerNeverSucceeds_ThrowsTimeout()
    {
        using var http = new HttpClient(new FakeHttpMessageHandler(HttpStatusCode.ServiceUnavailable));

        await Assert.ThrowsAsync<TimeoutException>(() =>
            BackendService.WaitForHealthAsync(http, "http://localhost:5000", timeoutSeconds: 1));
    }

    [Fact]
    public async Task WaitForHealthAsync_WhenServerThrows_EventuallyTimesOut()
    {
        using var http = new HttpClient(new ThrowingHandler());

        await Assert.ThrowsAsync<TimeoutException>(() =>
            BackendService.WaitForHealthAsync(http, "http://localhost:5000", timeoutSeconds: 1));
    }

    [Fact]
    public async Task WaitForHealthAsync_CallsOnWaiting_WhilePolling()
    {
        var messages = new List<string>();
        using var http = new HttpClient(new FakeHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            new HttpResponseMessage(HttpStatusCode.OK)));

        await BackendService.WaitForHealthAsync(http, "http://localhost:5000",
            onWaiting: msg => messages.Add(msg));

        Assert.Single(messages);
        Assert.Equal("Waiting for backend…", messages[0]);
    }

    // ── CheckVersionCompatibilityAsync ───────────────────────────────────────

    [Fact]
    public async Task CheckVersionCompatibilityAsync_WhenCompatible_DoesNotThrow()
    {
        using var http = new HttpClient(new FakeHttpMessageHandler(HttpStatusCode.OK));

        await BackendService.CheckVersionCompatibilityAsync(http, "http://localhost:5000", "0.8.1");
    }

    [Fact]
    public async Task CheckVersionCompatibilityAsync_WhenIncompatible_Throws()
    {
        using var http = new HttpClient(new FakeHttpMessageHandler(HttpStatusCode.BadRequest));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            BackendService.CheckVersionCompatibilityAsync(http, "http://localhost:5000", "0.8.1"));

        Assert.Contains("0.8.1", ex.Message);
    }

    [Fact]
    public async Task CheckVersionCompatibilityAsync_WhenServerReturns404_DoesNotThrow()
    {
        using var http = new HttpClient(new FakeHttpMessageHandler(HttpStatusCode.NotFound));

        await BackendService.CheckVersionCompatibilityAsync(http, "http://localhost:5000", "0.8.1");
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => throw new HttpRequestException("simulated failure");
    }
}
