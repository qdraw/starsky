using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.metaupdate.Services;

namespace starskytest.starsky.foundation.metaupdate.Services;

[TestClass]
public sealed class MetaUpdateBackgroundJobHandlerTest
{
    [TestMethod]
    public async Task ExecuteAsync_MissingPayload_ThrowsArgumentException()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var handler = new MetaUpdateBackgroundJobHandler(scopeFactory);

        // Act & Assert - null
        var exNull = await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => handler.ExecuteAsync(null, CancellationToken.None));
        Assert.AreEqual("Missing payload", exNull.Message);

        // Act & Assert - whitespace
        var exWhite = await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => handler.ExecuteAsync("   ", CancellationToken.None));
        Assert.AreEqual("Missing payload", exWhite.Message);
    }

    [TestMethod]
    public async Task ExecuteAsync_InvalidPayload_ThrowsArgumentException()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var handler = new MetaUpdateBackgroundJobHandler(scopeFactory);

        // Act & Assert - JSON 'null' deserializes to null -> should trigger Invalid payload
        var ex = await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => handler.ExecuteAsync("null", CancellationToken.None));
        Assert.AreEqual("Invalid payload", ex.Message);
    }
}

