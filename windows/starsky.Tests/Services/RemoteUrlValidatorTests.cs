using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Starsky.Desktop.Services;
using starsky.Tests.Helpers;

namespace starsky.Tests.Services;

public class RemoteUrlValidatorTests
{
    private static RemoteUrlValidator Create(HttpStatusCode status, string content = "") =>
        new RemoteUrlValidator(
            NullLogger<RemoteUrlValidator>.Instance,
            new HttpClient(new FakeHttpMessageHandler(status, content)));

    [Fact]
    public async Task ValidateAsync_EmptyString_ReturnsFalse()
    {
        var svc = Create(HttpStatusCode.OK);

        var result = await svc.ValidateAsync(string.Empty);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task ValidateAsync_InvalidScheme_ReturnsFalse()
    {
        var svc = Create(HttpStatusCode.OK);

        var result = await svc.ValidateAsync("ftp://example.com");

        Assert.False(result.Success);
        Assert.Contains("http", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidateAsync_WhenServerReturns200_ReturnsSuccess()
    {
        var svc = Create(HttpStatusCode.OK);

        var result = await svc.ValidateAsync("http://localhost:5000");

        Assert.True(result.Success);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task ValidateAsync_WhenServerReturns503_ReturnsSuccess()
    {
        var svc = Create(HttpStatusCode.ServiceUnavailable);

        var result = await svc.ValidateAsync("http://localhost:5000");

        Assert.True(result.Success);
    }

    [Fact]
    public async Task ValidateAsync_WhenServerReturnsOtherError_ReturnsFalse()
    {
        var svc = Create(HttpStatusCode.Forbidden);

        var result = await svc.ValidateAsync("http://localhost:5000");

        Assert.False(result.Success);
        Assert.Contains("403", result.Error);
    }

    [Fact]
    public async Task ValidateAsync_WhenRequestThrows_ReturnsFalse()
    {
        var handler = new ThrowingHttpMessageHandler();
        var svc = new RemoteUrlValidator(
            NullLogger<RemoteUrlValidator>.Instance,
            new HttpClient(handler));

        var result = await svc.ValidateAsync("http://localhost:5000");

        Assert.False(result.Success);
    }

    [Fact]
    public async Task ValidateAsync_TrailingSlash_DoesNotAffectResult()
    {
        var svc = Create(HttpStatusCode.OK);

        var withSlash = await svc.ValidateAsync("http://localhost:5000/");
        var withoutSlash = await svc.ValidateAsync("http://localhost:5000");

        Assert.Equal(withSlash.Success, withoutSlash.Success);
    }

    private sealed class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => throw new HttpRequestException("connection refused");
    }
}
