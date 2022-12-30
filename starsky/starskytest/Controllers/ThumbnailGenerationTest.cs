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
	public sealed class ThumbnailGenerationTest
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
				new FakeIWebLogger(), new FakeIWebSocketConnectionsService(), new FakeIThumbnailService());
			
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
			
			var selectorStorage = new FakeSelectorStorage(storage);
			var controller = new ThumbnailGenerationController(selectorStorage, new FakeIQuery(
					new List<FileIndexItem>{new FileIndexItem("/test.jpg")}
				), new FakeIWebLogger(), new FakeIWebSocketConnectionsService(), new FakeIThumbnailService(selectorStorage));

			await controller.WorkThumbnailGeneration("/");

			var folder = storage.GetAllFilesInDirectoryRecursive(
				"/").ToList();
			
			var name = Base32.Encode(System.Text.Encoding.UTF8.GetBytes("/"));
			Assert.AreEqual(1, folder.Count(p => p == "/"+ name + "@2000.jpg"));
		}
		
		[TestMethod]
		public async Task ThumbnailGenerationTest_CheckIfGenerated_Socket_Success()
		{
			var storage = new FakeIStorage(new List<string> {"/"}, new List<string> {"/test.jpg"},
				new List<byte[]> {CreateAnImage.Bytes});
			
			var socket = new FakeIWebSocketConnectionsService();
			var selectorStorage = new FakeSelectorStorage(storage);
			var controller = new ThumbnailGenerationController(selectorStorage, new FakeIQuery(
				new List<FileIndexItem>{new FileIndexItem("/test.jpg")}
			), new FakeIWebLogger(), socket, new FakeIThumbnailService(selectorStorage));

			await controller.WorkThumbnailGeneration("/");

			Assert.AreEqual(1, socket.FakeSendToAllAsync.Count(p => !p.StartsWith("[system]")));
		}
		
		[TestMethod]
		public async Task ThumbnailGenerationTest_CheckIfGenerated_Socket_NoResultsInDatabase()
		{
			var storage = new FakeIStorage(new List<string> {"/"}, new List<string> {"/test.jpg"},
				new List<byte[]> {CreateAnImage.Bytes});

			var socket = new FakeIWebSocketConnectionsService();
			var selectorStorage = new FakeSelectorStorage(storage);
			var controller = new ThumbnailGenerationController(selectorStorage, new FakeIQuery(
				new List<FileIndexItem>()), new FakeIWebLogger(), socket, new FakeIThumbnailService());

			await controller.WorkThumbnailGeneration("/");

			Assert.AreEqual(0, socket.FakeSendToAllAsync.Count);
		}

		[TestMethod]
		public async Task WorkItem_TestFailing()
		{
			var message = "[ThumbnailGenerationController] reading not allowed";
			
			var storage = new FakeIStorage();
			var selectorStorage = new FakeSelectorStorage(storage);

			var webLogger = new FakeIWebLogger();
			var controller = new ThumbnailGenerationController(selectorStorage, new FakeIQuery(), 
				webLogger, new FakeIWebSocketConnectionsService(), new FakeIThumbnailService(null,new UnauthorizedAccessException(message)));
			
			await controller.WorkThumbnailGeneration("/");

			Assert.IsTrue(webLogger.TrackedExceptions.FirstOrDefault().Item2.Contains(message));
		}
	}
}
