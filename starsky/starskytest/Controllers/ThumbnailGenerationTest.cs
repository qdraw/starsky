using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.storage.Services;
using starsky.foundation.worker.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Controllers
{
	[TestClass]
	public class ThumbnailGenerationTest
	{
		private readonly IBackgroundTaskQueue _bgTaskQueue;

		public ThumbnailGenerationTest()
		{
			var services = new ServiceCollection();
			services.AddSingleton<IHostedService, BackgroundQueuedHostedService>();
			services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

			var serviceProvider = services.BuildServiceProvider();
			_bgTaskQueue = serviceProvider.GetRequiredService<IBackgroundTaskQueue>();

		}

		[TestMethod]
		public async Task ThumbnailGenerationTest_CheckIfGenerated()
		{
			var storage = new FakeIStorage(new List<string> {"/"}, new List<string> {"/test.jpg"},
				new List<byte[]> {CreateAnImage.Bytes});

			var thumbStorage = new FakeIStorage();

			var selectorStorage = new FakeSelectorStorage(storage);
			var controller = new ThumbnailGenerationController(selectorStorage, _bgTaskQueue, new FakeIWebLogger());

			var json = controller.ThumbnailGeneration("/") as JsonResult;
			var result = json.Value as string;
			Assert.IsNotNull(result);

			await controller.WorkItem("/", storage, thumbStorage);

			var folder = thumbStorage.GetAllFilesInDirectoryRecursive(
				"/").ToList();
			Assert.AreEqual(1, folder.Count(p => !p.Contains("@")));
		}

		[TestMethod]
		public async Task TestFailing()
		{
			var message = "[ThumbnailGenerationController] reading not allowed";
			
			var storage = new FakeIStorage(new UnauthorizedAccessException(message));
			var selectorStorage = new FakeSelectorStorage(storage);

			var telemetry = new FakeIWebLogger();
			var controller = new ThumbnailGenerationController(selectorStorage, _bgTaskQueue,
				telemetry);
			await controller.WorkItem("/", storage, storage);

			Assert.IsTrue(telemetry.TrackedExceptions.FirstOrDefault().Item2.Contains(message));
		}
	}
}
