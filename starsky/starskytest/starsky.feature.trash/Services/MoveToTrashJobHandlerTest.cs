using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.trash.Services;
using starsky.foundation.database.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.trash.Services;

[TestClass]
public sealed class MoveToTrashJobHandlerTest
{
	[TestMethod]
	public async Task ExecuteAsync_MissingPayload_ThrowsArgumentException()
	{
		var fakeService = new FakeIMoveToTrashService(new List<FileIndexItem>());
		var handler = new MoveToTrashJobHandler(fakeService);

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
		var fakeService = new FakeIMoveToTrashService(new List<FileIndexItem>());
		var handler = new MoveToTrashJobHandler(fakeService);
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
