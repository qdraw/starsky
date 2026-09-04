using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Starsky.Desktop.Services;
using starsky.Tests.Helpers;

namespace starsky.Tests.Services;

[TestClass]
public class RemoteUrlValidatorTests
{
    private static RemoteUrlValidator Create(HttpStatusCode status, string content = "") =>
        new RemoteUrlValidator(
            NullLogger<RemoteUrlValidator>.Instance,
            new HttpClient(new FakeHttpMessageHandler(status, content)));

    [TestMethod]
    public async Task ValidateAsync_EmptyString_ReturnsFalse()
    {
        var svc = Create(HttpStatusCode.OK);

        var result = await svc.ValidateAsync(string.Empty);

        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public async Task ValidateAsync_InvalidScheme_ReturnsFalse()
    {
        var svc = Create(HttpStatusCode.OK);

        var result = await svc.ValidateAsync("ftp://example.com");

        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Error!.Contains("http", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task ValidateAsync_WhenServerReturns200_ReturnsSuccess()
    {
        var svc = Create(HttpStatusCode.OK);

        var result = await svc.ValidateAsync("http://localhost:5000");

        Assert.IsTrue(result.Success);
        Assert.IsNull(result.Error);
    }

    [TestMethod]
    public async Task ValidateAsync_WhenServerReturns503_ReturnsSuccess()
    {
        var svc = Create(HttpStatusCode.ServiceUnavailable);

        var result = await svc.ValidateAsync("http://localhost:5000");

        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public async Task ValidateAsync_WhenServerReturnsOtherError_ReturnsFalse()
    {
        var svc = Create(HttpStatusCode.Forbidden);

        var result = await svc.ValidateAsync("http://localhost:5000");

        Assert.IsFalse(result.Success);
        Assert.Contains("403", result.Error!);
    }

    [TestMethod]
    public async Task ValidateAsync_WhenRequestThrows_ReturnsFalse()
    {
        var handler = new ThrowingHttpMessageHandler();
        var svc = new RemoteUrlValidator(
            NullLogger<RemoteUrlValidator>.Instance,
            new HttpClient(handler));

        var result = await svc.ValidateAsync("http://localhost:5000");

        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public async Task ValidateAsync_TrailingSlash_DoesNotAffectResult()
    {
        var svc = Create(HttpStatusCode.OK);

        var withSlash = await svc.ValidateAsync("http://localhost:5000/");
        var withoutSlash = await svc.ValidateAsync("http://localhost:5000");

        Assert.AreEqual(withSlash.Success, withoutSlash.Success);
    }

    [TestMethod]
    public async Task ValidateAsync_HttpsScheme_IsAccepted()
    {
        var svc = Create(HttpStatusCode.OK);

        var result = await svc.ValidateAsync("https://example.com");

        Assert.IsTrue(result.Success);
    }

    private sealed class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => throw new HttpRequestException("connection refused");
    }
}
