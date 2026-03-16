using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.trash.Services;
using starskytest.FakeMocks;
using starsky.foundation.database.Models;

namespace starskytest.starsky.feature.trash.Services
{
    [TestClass]
    public sealed class MoveToTrashJobHandlerTest
    {
        [TestMethod]
        public async Task ExecuteAsync_MissingPayload_ThrowsArgumentException()
        {
            var fakeService = new FakeIMoveToTrashService(new System.Collections.Generic.List<FileIndexItem>());
            var handler = new MoveToTrashJobHandler(fakeService);

            try
            {
                await handler.ExecuteAsync(null, CancellationToken.None);
                Assert.Fail("Expected ArgumentException not thrown");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.Contains("Missing payload"));
                Assert.AreEqual("payloadJson", e.ParamName);
            }
        }

        [TestMethod]
        public async Task ExecuteAsync_InvalidPayload_ThrowsArgumentException()
        {
            var fakeService = new FakeIMoveToTrashService(new System.Collections.Generic.List<FileIndexItem>());
            var handler = new MoveToTrashJobHandler(fakeService);

            try
            {
                // pass the JSON literal null so JsonSerializer.Deserialize returns null
                await handler.ExecuteAsync("null", CancellationToken.None);
                Assert.Fail("Expected ArgumentException not thrown");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.Contains("Invalid payload"));
                Assert.AreEqual("payloadJson", e.ParamName);
            }
        }
    }
}


