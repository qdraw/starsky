using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Starsky.Desktop.Services;
using starsky.Tests.FakeCreateAn.CreateFakeStarskyExe;
using starsky.Tests.Helpers;

namespace starsky.Tests.Services;

[TestClass]
public class BackendServiceTests
{
    [TestMethod]
    public async Task StopAsync_WhenNotStarted_DoesNotThrow()
    {
        var svc = new BackendService(NullLogger<BackendService>.Instance);

        Exception? ex = null;
        try { await svc.StopAsync(); } catch (Exception e) { ex = e; }

        Assert.IsNull(ex);
    }

    [TestMethod]
    public void Dispose_WhenNotStarted_DoesNotThrow()
    {
        var svc = new BackendService(NullLogger<BackendService>.Instance);

        Exception? ex = null;
        try { svc.Dispose(); } catch (Exception e) { ex = e; }

        Assert.IsNull(ex);
    }

    [TestMethod]
    public void SetEnvironment_SetsAllRequiredKeys()
    {
        var env = new Dictionary<string, string?>();

        BackendService.SetEnvironment(env, 12345);

        Assert.AreEqual("http://localhost:12345", env["ASPNETCORE_URLS"]);
        Assert.AreEqual("true", env["app__NoAccountLocalhost"]);
        Assert.AreEqual("true", env["app__UseLocalDesktop"]);
        Assert.AreEqual("Administrator", env["app__AccountRegisterDefaultRole"]);
        Assert.AreEqual("false", env["app__Verbose"]);
        CollectionAssert.Contains(env.Keys.ToList(), "app__databaseConnection");
        CollectionAssert.Contains(env.Keys.ToList(), "app__tempFolder");
        CollectionAssert.Contains(env.Keys.ToList(), "app__appsettingspath");
    }

    [TestMethod]
    public void FindBackendExe_WhenNoExeExists_ReturnsNull()
    {
        var emptyDir = Path.Combine(Path.GetTempPath(), $"starsky-empty-{Guid.NewGuid()}");
        Directory.CreateDirectory(emptyDir);

        try
        {
            var result = BackendService.FindBackendExe(emptyDir);

            Assert.IsNull(result);
        }
        finally
        {
            Directory.Delete(emptyDir);
        }
    }

    [TestMethod]
    public void FindBackendExe_WhenExeExists_ReturnsPath()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"starsky-exe-{Guid.NewGuid()}");
        Directory.CreateDirectory(dir);
        var exePath = Path.Combine(dir, "starsky.exe");
        File.WriteAllBytes(exePath, Array.Empty<byte>());

        try
        {
            var result = BackendService.FindBackendExe(dir);

            Assert.AreEqual(exePath, result);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [TestMethod]
    public void FindBackendExe_WhenLinuxExeExists_ReturnsPath()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"starsky-linux-{Guid.NewGuid()}");
        Directory.CreateDirectory(dir);
        var exePath = Path.Combine(dir, "starsky");
        File.WriteAllBytes(exePath, Array.Empty<byte>());

        try
        {
            var result = BackendService.FindBackendExe(dir);

            Assert.AreEqual(exePath, result);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [TestMethod]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var svc = new BackendService(NullLogger<BackendService>.Instance);
        svc.Dispose();

        Exception? ex = null;
        try { svc.Dispose(); } catch (Exception e) { ex = e; }

        Assert.IsNull(ex);
    }

    [TestMethod]
    public void SetEnvironment_ContainsThumbnailAndSettingsPaths()
    {
        var env = new Dictionary<string, string?>();

        BackendService.SetEnvironment(env, 5000);

        CollectionAssert.Contains(env.Keys.ToList(), "app__thumbnailTempFolder");
        CollectionAssert.Contains(env.Keys.ToList(), "app__appsettingslocalpath");
        Assert.AreEqual("300", env["app__ThumbnailGenerationIntervalInMinutes"]);
    }

    [TestMethod]
    public async Task StartAsync_WhenExeNotFound_Throws()
    {
        // In the test environment ApplicationPaths.RuntimeDir does not contain a starsky.exe,
        // so LaunchAsync cannot find the backend and throws FileNotFoundException.
        if (BackendService.FindBackendExe(ApplicationPaths.RuntimeDir) != null)
        {
            return; // Runtime present in test env — skip
        }

        var svc = new BackendService(NullLogger<BackendService>.Instance);

        Exception? caught = null;
        try { await svc.StartAsync(5000); } catch (FileNotFoundException e) { caught = e; }
        Assert.IsInstanceOfType<FileNotFoundException>(caught);
    }

    // ── WaitForHealthAsync ────────────────────────────────────────────────────

    [TestMethod]
    public async Task WaitForHealthAsync_WhenServerReturns200_Returns()
    {
        using var http = new HttpClient(new FakeHttpMessageHandler(HttpStatusCode.OK));

        var result = await BackendService.WaitForHealthAsync(http, "http://localhost:5000");

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task WaitForHealthAsync_WhenServerNeverSucceeds_ThrowsTimeout()
    {
        using var http = new HttpClient(new FakeHttpMessageHandler(HttpStatusCode.ServiceUnavailable));

        Exception? caught = null;
        try { await BackendService.WaitForHealthAsync(http, "http://localhost:5000", timeoutSeconds: 1); } catch (TimeoutException e) { caught = e; }
        Assert.IsInstanceOfType<TimeoutException>(caught);
    }

    [TestMethod]
    public async Task WaitForHealthAsync_WhenServerThrows_EventuallyTimesOut()
    {
        using var http = new HttpClient(new ThrowingHandler());

        Exception? caught = null;
        try { await BackendService.WaitForHealthAsync(http, "http://localhost:5000", timeoutSeconds: 1); } catch (TimeoutException e) { caught = e; }
        Assert.IsInstanceOfType<TimeoutException>(caught);
    }

    [TestMethod]
    public async Task WaitForHealthAsync_CallsOnWaiting_WhilePolling()
    {
        var messages = new List<string>();
        using var http = new HttpClient(new FakeHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            new HttpResponseMessage(HttpStatusCode.OK)));

        await BackendService.WaitForHealthAsync(http, "http://localhost:5000",
            onWaiting: msg => messages.Add(msg));

        Assert.AreEqual(1, messages.Count);
        Assert.AreEqual("Waiting for backend…", messages[0]);
    }

    // ── CheckVersionCompatibilityAsync ───────────────────────────────────────

    [TestMethod]
    public async Task CheckVersionCompatibilityAsync_WhenCompatible_DoesNotThrow()
    {
        using var http = new HttpClient(new FakeHttpMessageHandler(HttpStatusCode.OK));

        await BackendService.CheckVersionCompatibilityAsync(http, "http://localhost:5000", "0.8.1");
    }

    [TestMethod]
    public async Task CheckVersionCompatibilityAsync_WhenIncompatible_Throws()
    {
        using var http = new HttpClient(new FakeHttpMessageHandler(HttpStatusCode.BadRequest));

        InvalidOperationException? ex = null;
        try { await BackendService.CheckVersionCompatibilityAsync(http, "http://localhost:5000", "0.8.1"); } catch (InvalidOperationException e) { ex = e; }
        Assert.IsNotNull(ex);

        Assert.IsTrue(ex.Message.Contains("0.8.1"));
    }

    [TestMethod]
    public async Task CheckVersionCompatibilityAsync_WhenServerReturns404_DoesNotThrow()
    {
        using var http = new HttpClient(new FakeHttpMessageHandler(HttpStatusCode.NotFound));

        await BackendService.CheckVersionCompatibilityAsync(http, "http://localhost:5000", "0.8.1");
    }

    // ── StartAsync / LaunchAsync (process launch) ────────────────────────────

    [TestMethod]
    public async Task StartAsync_WithFakeExe_StartsProcess()
    {
        var fakeExe = new CreateFakeStarskyExe();
        var runtimeDir = ApplicationPaths.RuntimeDir;
        var exeName = OperatingSystem.IsWindows() ? "starsky.exe" : "starsky";
        var destExe = Path.Combine(runtimeDir, exeName);
        Directory.CreateDirectory(runtimeDir);
        File.Copy(fakeExe.ExePath, destExe, overwrite: true);

        try
        {
            var svc = new BackendService(NullLogger<BackendService>.Instance);
            await svc.StartAsync(19999);
            await svc.StopAsync();
            Assert.IsNotNull(svc);
            svc.Dispose();
        }
        finally
        {
            File.Delete(destExe);
            if (!Directory.EnumerateFileSystemEntries(runtimeDir).Any())
            {
                Directory.Delete(runtimeDir);
            }
        }
    }

    [TestMethod]
    public async Task StartAsync_WithFakeExe_StopAsync_DoesNotThrow()
    {
        var fakeExe = new CreateFakeStarskyExe();
        var runtimeDir = ApplicationPaths.RuntimeDir;
        var exeName = OperatingSystem.IsWindows() ? "starsky.exe" : "starsky";
        var destExe = Path.Combine(runtimeDir, exeName);
        Directory.CreateDirectory(runtimeDir);
        File.Copy(fakeExe.ExePath, destExe, overwrite: true);

        try
        {
            var svc = new BackendService(NullLogger<BackendService>.Instance);
            await svc.StartAsync(19998);

            Exception? ex = null;
            try { await svc.StopAsync(); } catch (Exception e) { ex = e; }

            Assert.IsNull(ex);
            svc.Dispose();
        }
        finally
        {
            File.Delete(destExe);
            if (!Directory.EnumerateFileSystemEntries(runtimeDir).Any())
            {
                Directory.Delete(runtimeDir);
            }
        }
    }

    [TestMethod]
    public async Task StartAsync_WithFakeExe_Dispose_DoesNotThrow()
    {
        var fakeExe = new CreateFakeStarskyExe();
        var runtimeDir = ApplicationPaths.RuntimeDir;
        var exeName = OperatingSystem.IsWindows() ? "starsky.exe" : "starsky";
        var destExe = Path.Combine(runtimeDir, exeName);
        Directory.CreateDirectory(runtimeDir);
        File.Copy(fakeExe.ExePath, destExe, overwrite: true);

        try
        {
            var svc = new BackendService(NullLogger<BackendService>.Instance);
            await svc.StartAsync(19997);

            Exception? ex = null;
            try { svc.Dispose(); } catch (Exception e) { ex = e; }

            Assert.IsNull(ex);
        }
        finally
        {
            File.Delete(destExe);
            if (!Directory.EnumerateFileSystemEntries(runtimeDir).Any())
            {
                Directory.Delete(runtimeDir);
            }
        }
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => throw new HttpRequestException("simulated failure");
    }
}
