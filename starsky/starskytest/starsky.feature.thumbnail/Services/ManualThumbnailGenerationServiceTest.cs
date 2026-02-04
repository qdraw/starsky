using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.thumbnail.Services;
using starsky.foundation.storage.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.thumbnail.Services;

[TestClass]
public class ManualThumbnailGenerationServiceTest
{
	[TestMethod]
	public async Task ThumbnailGenerationTest_CheckIfGenerated()
	{
		var storage = new FakeIStorage(new List<string> { "/" }, new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var selectorStorage = new FakeSelectorStorage(storage);
		// new FakeIQuery(
		// 	new List<FileIndexItem>{new FileIndexItem("/test.jpg")}
		// )
		var controller = new ManualThumbnailGenerationService(new FakeIWebLogger(),
			new FakeIThumbnailSocketService(),
			new FakeIThumbnailService(selectorStorage), new FakeThumbnailBackgroundTaskQueue());

		await controller.WorkThumbnailGeneration("/");

		var folder = storage.GetAllFilesInDirectoryRecursive(
			"/").ToList();

		var name = Base32.Encode(Encoding.UTF8.GetBytes("/"));
		Assert.AreEqual(1, folder.Count(p => p == "/" + name + "@2000.jpg"));
	}

	[TestMethod]
	public async Task ThumbnailGenerationTest_CheckIfGenerated_Socket_Success()
	{
		var storage = new FakeIStorage(new List<string> { "/" }, new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var socket = new FakeIThumbnailSocketService();
		var selectorStorage = new FakeSelectorStorage(storage);
		var controller = new ManualThumbnailGenerationService(new FakeIWebLogger(),
			socket,
			new FakeIThumbnailService(selectorStorage), new FakeThumbnailBackgroundTaskQueue());

		await controller.WorkThumbnailGeneration("/");

		Assert.HasCount(1, socket.Results);
	}

	[TestMethod]
	public async Task ThumbnailGenerationTest_CheckIfGenerated_Socket_NoResultsInDatabase()
	{
		var socket = new FakeIWebSocketConnectionsService();
		var controller = new ManualThumbnailGenerationService(new FakeIWebLogger(),
			new FakeIThumbnailSocketService(),
			new FakeIThumbnailService(new FakeSelectorStorage()),
			new FakeThumbnailBackgroundTaskQueue());

		await controller.WorkThumbnailGeneration("/");

		Assert.IsEmpty(socket.FakeSendToAllAsync);
	}

	[TestMethod]
	public async Task WorkItem_TestFailing()
	{
		const string message = "[ThumbnailGenerationController] reading not allowed";

		var webLogger = new FakeIWebLogger();
		var controller = new ManualThumbnailGenerationService(webLogger,
			new ThumbnailSocketService(new FakeIQuery(), new FakeIWebSocketConnectionsService(),
				webLogger,
				new FakeINotificationQuery()), new FakeIThumbnailService(null,
				new UnauthorizedAccessException(message)), new FakeThumbnailBackgroundTaskQueue());

		await controller.WorkThumbnailGeneration("/");

		Assert.IsTrue(webLogger.TrackedExceptions.FirstOrDefault().Item2?.Contains(message));
	}
}
