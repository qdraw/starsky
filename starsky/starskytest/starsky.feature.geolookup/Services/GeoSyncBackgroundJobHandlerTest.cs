using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.geolookup.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.geolookup.Services;

[TestClass]
public sealed class GeoSyncBackgroundJobHandlerTest
{
    [TestMethod]
    public async Task ExecuteAsync_MissingPayload_ThrowsArgumentException()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var fakeLogger = new FakeIWebLogger();
        var handler = new GeoSyncBackgroundJobHandler(scopeFactory, fakeLogger);

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

        Assert.IsNotNull(caught);
        Assert.Contains("Missing payload", caught.Message);
        // Implementation does not set ParamName for this handler
        Assert.IsNull(caught.ParamName);
    }

    [TestMethod]
    public async Task ExecuteAsync_InvalidPayload_ThrowsArgumentException()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var fakeLogger = new FakeIWebLogger();
        var handler = new GeoSyncBackgroundJobHandler(scopeFactory, fakeLogger);

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

        Assert.IsNotNull(caught);
        Assert.Contains("Invalid payload", caught.Message);
        // Implementation does not set ParamName for this handler
        Assert.IsNull(caught.ParamName);
    }
}

