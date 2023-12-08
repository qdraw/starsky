using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.thumbnail.Services;
using starsky.foundation.database.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.thumbnailgeneration.Models;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.thumbnail.Services 
{

	[TestClass]
	public class ManualThumbnailGenerationServiceTest
	{
	
		[TestMethod]
		public async Task ThumbnailGenerationTest_CheckIfGenerated()
		{
			var storage = new FakeIStorage(new List<string> {"/"}, new List<string> {"/test.jpg"},
				new List<byte[]> {CreateAnImage.Bytes.ToArray()});
			
			var selectorStorage = new FakeSelectorStorage(storage);
			var controller = new ManualThumbnailGenerationService( new FakeIQuery(
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
				new List<byte[]> {CreateAnImage.Bytes.ToArray()});
			
			var socket = new FakeIWebSocketConnectionsService();
			var selectorStorage = new FakeSelectorStorage(storage);
			var controller = new ManualThumbnailGenerationService(new FakeIQuery(
				new List<FileIndexItem>{new FileIndexItem("/test.jpg")}
			), new FakeIWebLogger(), socket, new FakeIThumbnailService(selectorStorage), new FakeThumbnailBackgroundTaskQueue());

			await controller.WorkThumbnailGeneration("/");

			Assert.AreEqual(1, socket.FakeSendToAllAsync.Count(p => !p.StartsWith("[system]")));
		}

		[TestMethod]
		public async Task ThumbnailGenerationTest_CheckIfGenerated_Socket_NoResultsInDatabase()
		{
			var socket = new FakeIWebSocketConnectionsService();
			var controller = new ManualThumbnailGenerationService( new FakeIQuery(
				new List<FileIndexItem>()), new FakeIWebLogger(), socket, new FakeIThumbnailService(), new FakeThumbnailBackgroundTaskQueue());

			await controller.WorkThumbnailGeneration("/");

			Assert.AreEqual(0, socket.FakeSendToAllAsync.Count);
		}

		[TestMethod]
		public async Task WorkItem_TestFailing()
		{
			const string message = "[ThumbnailGenerationController] reading not allowed";

			var webLogger = new FakeIWebLogger();
			var controller = new ManualThumbnailGenerationService(new FakeIQuery(), 
				webLogger, new FakeIWebSocketConnectionsService(), new FakeIThumbnailService(null,
					new UnauthorizedAccessException(message)), new FakeThumbnailBackgroundTaskQueue());
			
			await controller.WorkThumbnailGeneration("/");

			Assert.IsTrue(webLogger.TrackedExceptions.FirstOrDefault().Item2.Contains(message));
		}

		[TestMethod]
		public void WhichFilesNeedToBePushedForUpdate_NothingToUpdate()
		{
			var result = ManualThumbnailGenerationService.WhichFilesNeedToBePushedForUpdates(
				new List<GenerationResultModel>(), new List<FileIndexItem>());
			Assert.AreEqual(0, result.Count);
		}
		
		
		[TestMethod]
		public void WhichFilesNeedToBePushedForUpdate_DoesNotExistInFilesList()
		{
			var result = ManualThumbnailGenerationService.WhichFilesNeedToBePushedForUpdates(
				new List<GenerationResultModel>
				{
					new GenerationResultModel{SubPath = "/test.jpg", Success = true}
				}, new List<FileIndexItem>());
			
			Assert.AreEqual(0, result.Count);
		}

		[TestMethod]
		public void WhichFilesNeedToBePushedForUpdate_DeletedSoIgnored()
		{
			var result = ManualThumbnailGenerationService.WhichFilesNeedToBePushedForUpdates(
				new List<GenerationResultModel>
				{
					new GenerationResultModel{SubPath = "/test.jpg", Success = true}
				}, new List<FileIndexItem>{new FileIndexItem("/test.jpg"){
					Status = FileIndexItem.ExifStatus.Ok,
					Tags = TrashKeyword.TrashKeywordString
				}});
			
			Assert.AreEqual(0, result.Count);
		}
		
		
		[TestMethod]
		public void WhichFilesNeedToBePushedForUpdate_ShouldMap()
		{
			var result = ManualThumbnailGenerationService.WhichFilesNeedToBePushedForUpdates(
				new List<GenerationResultModel>
				{
					new GenerationResultModel{SubPath = "/test.jpg", Success = true}
				}, new List<FileIndexItem>{new FileIndexItem("/test.jpg")});
			
			Assert.AreEqual(1, result.Count);
		}
	}
}
