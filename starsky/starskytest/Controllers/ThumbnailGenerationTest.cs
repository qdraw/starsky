using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.database.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Controllers
{
	[TestClass]
	public class ThumbnailGenerationTest
	{
		private readonly IUpdateBackgroundTaskQueue _bgTaskQueue;

		public ThumbnailGenerationTest()
		{
			var services = new ServiceCollection();
			services.AddSingleton<IHostedService, UpdateBackgroundQueuedHostedService>();
			services.AddSingleton<IUpdateBackgroundTaskQueue, UpdateBackgroundTaskQueue>();

			var serviceProvider = services.BuildServiceProvider();
			_bgTaskQueue = serviceProvider.GetRequiredService<IUpdateBackgroundTaskQueue>();

		}

		[TestMethod]
		public async Task ThumbnailGeneration_Endpoint()
		{
			var selectorStorage = new FakeSelectorStorage(new FakeIStorage(new List<string>{"/"}));
			var controller = new ThumbnailGenerationController(selectorStorage, new FakeIQuery(), 
				new FakeIWebLogger(), new FakeIWebSocketConnectionsService());
			
			var json = await controller.ThumbnailGeneration("/") as JsonResult;
			var result = json.Value as string;
			
			Assert.IsNotNull(result);
			Assert.AreEqual("Job started", result);
		}

		[TestMethod]
		public async Task ThumbnailGenerationTest_CheckIfGenerated()
		{
			var storage = new FakeIStorage(new List<string> {"/"}, new List<string> {"/test.jpg"},
				new List<byte[]> {CreateAnImage.Bytes});

			var thumbStorage = new FakeIStorage();

			var selectorStorage = new FakeSelectorStorage(storage);
			var controller = new ThumbnailGenerationController(selectorStorage, new FakeIQuery(
					new List<FileIndexItem>{new FileIndexItem("/test.jpg")}
				), new FakeIWebLogger(), new FakeIWebSocketConnectionsService());

			await controller.WorkItem("/", storage, thumbStorage);

			var folder = thumbStorage.GetAllFilesInDirectoryRecursive(
				"/").ToList();
			
			Assert.AreEqual(1, folder.Count(p => !p.Contains("@")));
		}
		
		[TestMethod]
		public async Task ThumbnailGenerationTest_CheckIfGenerated_Socket_Success()
		{
			var storage = new FakeIStorage(new List<string> {"/"}, new List<string> {"/test.jpg"},
				new List<byte[]> {CreateAnImage.Bytes});

			var thumbStorage = new FakeIStorage();

			var socket = new FakeIWebSocketConnectionsService();
			var selectorStorage = new FakeSelectorStorage(storage);
			var controller = new ThumbnailGenerationController(selectorStorage, new FakeIQuery(
				new List<FileIndexItem>{new FileIndexItem("/test.jpg")}
			), new FakeIWebLogger(), socket);

			await controller.WorkItem("/", storage, thumbStorage);

			Assert.AreEqual(1, socket.FakeSendToAllAsync.Count(p => !p.StartsWith("[system]")));
		}
		
		[TestMethod]
		public async Task ThumbnailGenerationTest_CheckIfGenerated_Socket_NoResultsInDatabase()
		{
			var storage = new FakeIStorage(new List<string> {"/"}, new List<string> {"/test.jpg"},
				new List<byte[]> {CreateAnImage.Bytes});

			var thumbStorage = new FakeIStorage();

			var socket = new FakeIWebSocketConnectionsService();
			var selectorStorage = new FakeSelectorStorage(storage);
			var controller = new ThumbnailGenerationController(selectorStorage, new FakeIQuery(
				new List<FileIndexItem>()), new FakeIWebLogger(), socket);

			await controller.WorkItem("/", storage, thumbStorage);

			Assert.AreEqual(0, socket.FakeSendToAllAsync.Count);
		}

		[TestMethod]
		public async Task WorkItem_TestFailing()
		{
			var message = "[ThumbnailGenerationController] reading not allowed";
			
			var storage = new FakeIStorage(new UnauthorizedAccessException(message));
			var selectorStorage = new FakeSelectorStorage(storage);

			var webLogger = new FakeIWebLogger();
			var controller = new ThumbnailGenerationController(selectorStorage, new FakeIQuery(), 
				webLogger, new FakeIWebSocketConnectionsService());
			
			await controller.WorkItem("/", storage, storage);

			Assert.IsTrue(webLogger.TrackedExceptions.FirstOrDefault().Item2.Contains(message));
		}
	}
}
