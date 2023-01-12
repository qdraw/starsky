using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.feature.thumbnail.Services;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Services;
using starskytest.FakeMocks;

namespace starskytest.Controllers
{
	[TestClass]
	public sealed class ThumbnailGenerationControllerTest
	{
		public ThumbnailGenerationControllerTest()
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
			var controller = new ThumbnailGenerationController(selectorStorage, new ThumbnailGenerationService( new FakeIQuery(), 
				new FakeIWebLogger(), new FakeIWebSocketConnectionsService(), new FakeIThumbnailService(), new FakeThumbnailBackgroundTaskQueue()));
			
			var json = await controller.ThumbnailGeneration("/") as JsonResult;
			Assert.IsNotNull(json);
			var result = json!.Value as string;
			
			Assert.IsNotNull(result);
			Assert.AreEqual("Job started", result);
		}

	}
}
