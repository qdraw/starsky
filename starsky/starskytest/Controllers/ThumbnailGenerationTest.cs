using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
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
			var controller = new ThumbnailGenerationController(selectorStorage, _bgTaskQueue,
				new FakeTelemetryService());

			var json = controller.ThumbnailGeneration("/") as JsonResult;
			var result = json.Value as string;
			Assert.IsNotNull(result);

			await controller.WorkItem("/", storage, thumbStorage);

			Assert.AreEqual(1, thumbStorage.GetAllFilesInDirectoryRecursive("/").Count());
		}

		[TestMethod]
		public async Task TestFailing()
		{
			var message = "reading not allowed";
			var storage = new FakeIStorage(new UnauthorizedAccessException(message));
			var selectorStorage = new FakeSelectorStorage(storage);

			var telemetry = new FakeTelemetryService();
			var controller = new ThumbnailGenerationController(selectorStorage, _bgTaskQueue,
				telemetry);
			await controller.WorkItem("/", storage, storage);

			Assert.AreEqual(message,telemetry.TrackedExceptions.FirstOrDefault().Message);
		}
	}
}
