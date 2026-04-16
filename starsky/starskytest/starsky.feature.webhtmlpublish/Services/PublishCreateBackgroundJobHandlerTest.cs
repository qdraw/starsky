using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Services;
using starskytest.FakeMocks;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.feature.webhtmlpublish.Services;

[TestClass]
public sealed class PublishCreateBackgroundJobHandlerTest
{
    [TestMethod]
    public async Task ExecuteAsync_MissingPayload_ThrowsArgumentException()
    {
        var fakePublishService = new FakeIWebHtmlPublishService();
        var handler = new PublishCreateBackgroundJobHandler(fakePublishService, new AppSettings(), new FakeIWebLogger());

        ArgumentException? caught = null;
        try
        {
            await handler.ExecuteAsync(null, CancellationToken.None);
            Assert.Fail("Expected ArgumentException not thrown");
        }
        catch ( ArgumentException e )
        {
            caught = e;
        }

        Assert.Contains("Missing payload", caught.Message);
        Assert.AreEqual("payloadJson", caught.ParamName);
    }

    [TestMethod]
    public async Task ExecuteAsync_InvalidPayload_ThrowsArgumentException()
    {
        var fakePublishService = new FakeIWebHtmlPublishService();
        var handler = new PublishCreateBackgroundJobHandler(fakePublishService, new AppSettings(), new FakeIWebLogger());

        ArgumentException? caught = null;
        try
        {
            // pass the JSON literal null so JsonSerializer.Deserialize returns null
            await handler.ExecuteAsync("null", CancellationToken.None);
            Assert.Fail("Expected ArgumentException not thrown");
        }
        catch ( ArgumentException e )
        {
            caught = e;
        }

        Assert.Contains("Invalid payload", caught.Message);
        Assert.AreEqual("payloadJson", caught.ParamName);
    }
}

