using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.export.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.export.Services;

[TestClass]
public sealed class ExportBackgroundJobHandlerTest
{
    [TestMethod]
    public async Task ExecuteAsync_MissingPayload_ThrowsArgumentException()
    {
        var service = new FakeIExport(new System.Collections.Generic.Dictionary<string, bool>());
        var handler = new ExportBackgroundJobHandler(service);

        var ex = await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
            await handler.ExecuteAsync(null, CancellationToken.None));

        Assert.AreEqual("Missing payload", ex.Message);
    }

    [TestMethod]
    public async Task ExecuteAsync_InvalidPayload_ThrowsArgumentException()
    {
        var service = new FakeIExport(new System.Collections.Generic.Dictionary<string, bool>());
        var handler = new ExportBackgroundJobHandler(service);

        // pass the JSON literal null so JsonSerializer.Deserialize returns null
        var ex = await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
            await handler.ExecuteAsync("null", CancellationToken.None));

        Assert.AreEqual("Invalid payload", ex.Message);
    }
}

