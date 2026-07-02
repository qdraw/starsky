using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Text;
using Starsky.Tests.TestHelpers;
using Starsky.Windows.Services;

namespace Starsky.Tests.Services;

[TestClass]
public class BackendApiClientTests
{
    [TestMethod]
    public async Task ValidateRemoteUrlAsync_ReturnsInvalid_ForMalformedUrl()
    {
        var client = new BackendApiClient(StubHttpMessageHandler.CreateClient(HttpStatusCode.OK));

        var result = await client.ValidateRemoteUrlAsync("not a url", CancellationToken.None);

        Assert.IsFalse(result.IsValid);
        Assert.IsFalse(result.IsLocal);
        Assert.AreEqual("not a url", result.Location);
        Assert.AreEqual("Invalid URL", result.Reason);
    }

    [TestMethod]
    public async Task ValidateRemoteUrlAsync_ReturnsInvalid_ForUnsupportedScheme()
    {
        var client = new BackendApiClient(StubHttpMessageHandler.CreateClient(HttpStatusCode.OK));

        var result = await client.ValidateRemoteUrlAsync("ftp://example.test", CancellationToken.None);

        Assert.IsFalse(result.IsValid);
        Assert.IsFalse(result.IsLocal);
        Assert.AreEqual("ftp://example.test", result.Location);
        Assert.AreEqual("Only HTTP and HTTPS are supported", result.Reason);
    }

    [DataTestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    public async Task ValidateRemoteUrlAsync_ReturnsValid_ForAcceptedHealthStatuses(HttpStatusCode statusCode)
    {
        var client = new BackendApiClient(StubHttpMessageHandler.CreateClient(statusCode));

        var result = await client.ValidateRemoteUrlAsync(" https://example.test/ ", CancellationToken.None);

        Assert.IsTrue(result.IsValid);
        Assert.IsFalse(result.IsLocal);
        Assert.AreEqual("https://example.test", result.Location);
        Assert.IsNull(result.Reason);
    }

    [TestMethod]
    public async Task ValidateRemoteUrlAsync_ReturnsInvalid_ForUnexpectedHealthStatus()
    {
        var client = new BackendApiClient(StubHttpMessageHandler.CreateClient(HttpStatusCode.NotFound));

        var result = await client.ValidateRemoteUrlAsync("https://example.test", CancellationToken.None);

        Assert.IsFalse(result.IsValid);
        Assert.IsFalse(result.IsLocal);
        Assert.AreEqual("https://example.test", result.Location);
        Assert.AreEqual("Health endpoint returned 404", result.Reason);
    }

    [TestMethod]
    public async Task ValidateRemoteUrlAsync_ReturnsExceptionMessage_WhenRequestFails()
    {
        var httpClient = new HttpClient(new StubHttpMessageHandler((_, _) => throw new HttpRequestException("boom")));
        var client = new BackendApiClient(httpClient);

        var result = await client.ValidateRemoteUrlAsync("https://example.test", CancellationToken.None);

        Assert.IsFalse(result.IsValid);
        Assert.IsFalse(result.IsLocal);
        Assert.AreEqual("https://example.test", result.Location);
        Assert.AreEqual("boom", result.Reason);
    }

    [TestMethod]
    public async Task GetDetailViewAsync_ReturnsNull_WhenPayloadHasNoFileIndexItem()
    {
        var json = "{\"other\":{}}";
        var httpClient = new HttpClient(new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            })));
        var client = new BackendApiClient(httpClient);

        var result = await client.GetDetailViewAsync(new Uri("https://example.test"), "/x/y.jpg", CancellationToken.None);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetDetailViewAsync_MapsCollectionAndSidecarArrays()
    {
        var json = """
                   {
                     "fileIndexItem": {
                       "parentDirectory": "root/2026",
                       "fileCollectionName": "image",
                       "collectionPaths": ["root/2026/image.jpg", "root/2026/image.xmp"],
                       "sidecarExtensionsList": ["xmp"]
                     }
                   }
                   """;
        var httpClient = new HttpClient(new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            })));
        var client = new BackendApiClient(httpClient);

        var result = await client.GetDetailViewAsync(new Uri("https://example.test"), "/x/y.jpg", CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual("root/2026", result.ParentDirectory);
        Assert.AreEqual("image", result.FileCollectionName);
        CollectionAssert.AreEqual(new[] { "root/2026/image.jpg", "root/2026/image.xmp" }, result.CollectionPaths.ToArray());
        CollectionAssert.AreEqual(new[] { "xmp" }, result.SidecarExtensionsList.ToArray());
    }

    [TestMethod]
    public async Task GetDetailViewAsync_UsesEmptyLists_WhenOptionalArraysMissing()
    {
        var json = """
                   {
                     "fileIndexItem": {
                       "parentDirectory": "root/2026",
                       "fileCollectionName": "image"
                     }
                   }
                   """;
        var httpClient = new HttpClient(new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            })));
        var client = new BackendApiClient(httpClient);

        var result = await client.GetDetailViewAsync(new Uri("https://example.test"), "/x/y.jpg", CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.CollectionPaths.Count);
        Assert.AreEqual(0, result.SidecarExtensionsList.Count);
    }

    [TestMethod]
    public async Task DownloadToFileAsync_CreatesDirectoryAndWritesPayload()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "starsky-tests", Guid.NewGuid().ToString("N"));
        var targetFile = Path.Combine(tempRoot, "nested", "payload.bin");
        const string body = "hello world";

        try
        {
            var httpClient = new HttpClient(new StubHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(body, Encoding.UTF8, "text/plain"),
                })));
            var client = new BackendApiClient(httpClient);

            await client.DownloadToFileAsync(new Uri("https://example.test/file"), targetFile, CancellationToken.None);

            Assert.IsTrue(File.Exists(targetFile));
            var actual = await File.ReadAllTextAsync(targetFile);
            Assert.AreEqual(body, actual);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }
    }
}

