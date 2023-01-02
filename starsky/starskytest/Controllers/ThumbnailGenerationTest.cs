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
using starsky.foundation.thumbnailgeneration.Models;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Services;
using starsky.foundation.worker.ThumbnailServices;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Controllers
{
	[TestClass]
	public sealed class ThumbnailGenerationTest
	{
		public ThumbnailGenerationTest()
		{
			var services = new ServiceCollection();
			services.AddSingleton<IHostedService, UpdateBackgroundQueuedHostedService>();
			services.AddSingleton<IUpdateBackgroundTaskQueue, UpdateBackgroundTaskQueue>();

			var serviceProvider = services.BuildServiceProvider();
			serviceProvider.GetRequiredService<IUpdateBackgroundTaskQueue>();

		}

		[TestMethod]
		public async Task ThumbnailGeneration_Endpoint()
		{
			var selectorStorage = new FakeSelectorStorage(new FakeIStorage(new List<string>{"/"}));
			var controller = new ThumbnailGenerationController(selectorStorage, new FakeIQuery(), 
				new FakeIWebLogger(), new FakeIWebSocketConnectionsService(), new FakeIThumbnailService(), new FakeThumbnailBackgroundTaskQueue());
			
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
				), new FakeIWebLogger(), new FakeIWebSocketConnectionsService(), new FakeIThumbnailService(selectorStorage), new FakeThumbnailBackgroundTaskQueue());

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
			), new FakeIWebLogger(), socket, new FakeIThumbnailService(selectorStorage), new FakeThumbnailBackgroundTaskQueue());

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
				new List<FileIndexItem>()), new FakeIWebLogger(), socket, new FakeIThumbnailService(), new FakeThumbnailBackgroundTaskQueue());

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
				webLogger, new FakeIWebSocketConnectionsService(), new FakeIThumbnailService(null,
					new UnauthorizedAccessException(message)), new FakeThumbnailBackgroundTaskQueue());
			
			await controller.WorkThumbnailGeneration("/");

			Assert.IsTrue(webLogger.TrackedExceptions.FirstOrDefault().Item2.Contains(message));
		}

		[TestMethod]
		public void WhichFilesNeedToBePushedForUpdate_NothingToUpdate()
		{
			var result = ThumbnailGenerationController.WhichFilesNeedToBePushedForUpdates(
				new List<GenerationResultModel>(), new List<FileIndexItem>());
			Assert.AreEqual(0, result.Count);
		}
		
		
		[TestMethod]
		public void WhichFilesNeedToBePushedForUpdate_DoesNotExistInFilesList()
		{
			var result = ThumbnailGenerationController.WhichFilesNeedToBePushedForUpdates(
				new List<GenerationResultModel>
				{
					new GenerationResultModel{SubPath = "/test.jpg", Success = true}
				}, new List<FileIndexItem>());
			
			Assert.AreEqual(0, result.Count);
		}

		[TestMethod]
		public void WhichFilesNeedToBePushedForUpdate_DeletedSoIgnored()
		{
			var result = ThumbnailGenerationController.WhichFilesNeedToBePushedForUpdates(
				new List<GenerationResultModel>
				{
					new GenerationResultModel{SubPath = "/test.jpg", Success = true}
				}, new List<FileIndexItem>{new FileIndexItem("/test.jpg"){Tags = "!delete!"}});
			
			Assert.AreEqual(0, result.Count);
		}
		
		
		[TestMethod]
		public void WhichFilesNeedToBePushedForUpdate_ShouldMap()
		{
			var result = ThumbnailGenerationController.WhichFilesNeedToBePushedForUpdates(
				new List<GenerationResultModel>
				{
					new GenerationResultModel{SubPath = "/test.jpg", Success = true}
				}, new List<FileIndexItem>{new FileIndexItem("/test.jpg")});
			
			Assert.AreEqual(1, result.Count);
		}
	}
}
