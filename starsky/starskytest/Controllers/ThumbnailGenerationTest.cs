using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starskycore.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

[assembly: InternalsVisibleTo("starskytest")]
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
				new List<byte[]>{CreateAnImage.Bytes});

			var thumbStorage = new FakeIStorage();
			
			var selectorStorage = new FakeSelectorStorage(storage);
			var controller = new ThumbnailGenerationController(selectorStorage, _bgTaskQueue,
				new FakeTelemetryService());

			var json = await controller.ThumbnailGeneration("/") as JsonResult;
			var result = json.Value as string;
			Assert.IsNotNull(result);
				
			await controller.WorkItem("/", storage, thumbStorage);

			Assert.AreEqual(1, thumbStorage.GetAllFilesInDirectoryRecursive("/").Count());
		}
	}
}
