using Microsoft.Extensions.Logging.Abstractions;
using Starsky.Desktop.Services;

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
}
