using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Metrics;
using starsky.foundation.worker.Services;
using starsky.foundation.writemeta.Interfaces;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.Controllers
{
	[TestClass]
	public sealed class PublishControllerTest
	{
		private readonly IUpdateBackgroundTaskQueue _bgTaskQueue;

		public PublishControllerTest()
		{
			// Inject Fake Exiftool; dependency injection
			var services = new ServiceCollection();
			services.AddSingleton<IExifTool, FakeExifTool>();

			// Fake the readmeta output
			services.AddSingleton<IReadMeta, FakeReadMeta>();

			// metrics
			services.AddSingleton<IMeterFactory, FakeIMeterFactory>();
			services.AddSingleton<UpdateBackgroundQueuedMetrics>();

			// Inject Config helper
			services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
			// random config
			var createAnImage = new CreateAnImage();
			var dict = new Dictionary<string, string?>
			{
				{ "App:StorageFolder", createAnImage.BasePath },
				{ "App:ThumbnailTempFolder", createAnImage.BasePath },
				{ "App:Verbose", "true" }
			};
			// Start using dependency injection
			var builder = new ConfigurationBuilder();
			// Add random config to dependency injection
			builder.AddInMemoryCollection(dict);
			// build config
			var configuration = builder.Build();
			// inject config as object to a service
			services.ConfigurePoCo<AppSettings>(configuration.GetSection("App"));

			// Add Background services
			services.AddSingleton<IHostedService, UpdateBackgroundQueuedHostedService>();
			services.AddSingleton<IUpdateBackgroundTaskQueue, UpdateBackgroundTaskQueue>();

			// build the service
			var serviceProvider = services.BuildServiceProvider();
			// get the service
			serviceProvider.GetRequiredService<AppSettings>();

			// get the background helper
			_bgTaskQueue = serviceProvider.GetRequiredService<IUpdateBackgroundTaskQueue>();
		}

		[TestMethod]
		public void PublishGet_List()
		{
			var controller = new PublishController(new AppSettings(), new FakeIPublishPreflight(),
				new FakeIWebHtmlPublishService(), new FakeIMetaInfo(null!),
				new FakeSelectorStorage(),
				_bgTaskQueue, new FakeIWebLogger());

			var actionResult = controller.PublishGet() as JsonResult;
			var result = actionResult?.Value as IEnumerable<KeyValuePair<string, bool>>;

			Assert.AreEqual("test", result?.FirstOrDefault().Key);
		}

		[TestMethod]
		public async Task PublishCreate_newItem()
		{
			var controller = new PublishController(new AppSettings(), new FakeIPublishPreflight(),
				new FakeIWebHtmlPublishService(),
				new FakeIMetaInfo(new List<FileIndexItem>
				{
					new FileIndexItem("/test.jpg") { Status = FileIndexItem.ExifStatus.Ok }
				}),
				new FakeSelectorStorage(),
				_bgTaskQueue, new FakeIWebLogger());

			var actionResult = await controller.PublishCreateAsync("/test.jpg",
				"test", "test", true) as JsonResult;
			var result = actionResult?.Value as string;

			Assert.AreEqual("test", result);
		}

		[TestMethod]
		public async Task PublishCreate_newItem_readonly()
		{
			var controller = new PublishController(new AppSettings(), new FakeIPublishPreflight(),
				new FakeIWebHtmlPublishService(),
				new FakeIMetaInfo(new List<FileIndexItem>
				{
					new FileIndexItem("/test.jpg")
					{
						Status = FileIndexItem.ExifStatus.ReadOnly
					}
				}),
				new FakeSelectorStorage(),
				_bgTaskQueue, new FakeIWebLogger());

			var actionResult = await controller.PublishCreateAsync("/test.jpg",
				"test", "test", true) as JsonResult;
			var result = actionResult?.Value as string;

			Assert.AreEqual("test", result);
		}

		[TestMethod]
		public async Task PublishCreate_FakeBg_Expect_Generate_FakeZip_newItem()
		{
			var fakeBg = new FakeIUpdateBackgroundTaskQueue();
			var fakeIWebHtmlPublishService = new FakeIWebHtmlPublishService();
			var controller = new PublishController(new AppSettings(), new FakeIPublishPreflight(),
				fakeIWebHtmlPublishService,
				new FakeIMetaInfo(new List<FileIndexItem>
				{
					new FileIndexItem("/test.jpg") { Status = FileIndexItem.ExifStatus.Ok }
				}),
				new FakeSelectorStorage(),
				fakeBg, new FakeIWebLogger());

			await controller.PublishCreateAsync("/test.jpg",
				"test", "test", true);

			Assert.AreEqual(1, fakeIWebHtmlPublishService.ItemNamesGenerateZip.Count);
			Assert.AreEqual("test", fakeIWebHtmlPublishService.ItemNamesGenerateZip[0]);
		}

		[TestMethod]
		public async Task PublishCreate_NotFound()
		{
			var controller = new PublishController(new AppSettings(),
				new FakeIPublishPreflight(),
				new FakeIWebHtmlPublishService(),
				new FakeIMetaInfo(
					new List<FileIndexItem>
					{
						new FileIndexItem("/test.jpg")
						{
							Status = FileIndexItem.ExifStatus.NotFoundNotInIndex
						}
					}
				),
				new FakeSelectorStorage(),
				_bgTaskQueue, new FakeIWebLogger());

			var actionResult = await controller.PublishCreateAsync("/not-found.jpg",
				"test", "test", true) as NotFoundObjectResult;

			Assert.AreEqual(404, actionResult?.StatusCode);
		}


		[TestMethod]
		public async Task PublishCreate_InvalidResult()
		{
			var controller = new PublishController(new AppSettings(),
				new FakeIPublishPreflight(new List<AppSettingsPublishProfiles>(), false),
				new FakeIWebHtmlPublishService(),
				new FakeIMetaInfo(
					new List<FileIndexItem>
					{
						new FileIndexItem("/test.jpg")
						{
							Status = FileIndexItem.ExifStatus.NotFoundNotInIndex
						}
					}
				),
				new FakeSelectorStorage(),
				_bgTaskQueue, new FakeIWebLogger());

			var actionResult = await controller.PublishCreateAsync("/not-found.jpg",
				"test", "test", true) as BadRequestObjectResult;

			Assert.AreEqual(400, actionResult?.StatusCode);
		}

		[TestMethod]
		public async Task PublishCreate_existItem_NoForce()
		{
			var appSettings =
				new AppSettings { TempFolder = Path.DirectorySeparatorChar.ToString() };
			var storage = new FakeIStorage(
				new List<string> { Path.DirectorySeparatorChar + "test" },
				new List<string> { Path.DirectorySeparatorChar + "test.zip" });

			var controller = new PublishController(appSettings, new FakeIPublishPreflight(),
				new FakeIWebHtmlPublishService(),
				new FakeIMetaInfo(new List<FileIndexItem>
				{
					new FileIndexItem("/test.jpg") { Status = FileIndexItem.ExifStatus.Ok }
				}),
				new FakeSelectorStorage(storage),
				_bgTaskQueue, new FakeIWebLogger());

			var actionResult = await controller.PublishCreateAsync("/test.jpg",
				"test", "test") as ConflictObjectResult;
			var result = actionResult?.Value as string;

			Assert.AreEqual("name test exist", result);
		}

		[TestMethod]
		public async Task PublishCreate_existItem_Force()
		{
			var appSettings =
				new AppSettings { TempFolder = Path.DirectorySeparatorChar.ToString() };
			var storage = new FakeIStorage(
				new List<string> { Path.DirectorySeparatorChar + "test" },
				new List<string> { Path.DirectorySeparatorChar + "test.zip" });

			var controller = new PublishController(appSettings, new FakeIPublishPreflight(),
				new FakeIWebHtmlPublishService(),
				new FakeIMetaInfo(new List<FileIndexItem>
				{
					new FileIndexItem("/test.jpg") { Status = FileIndexItem.ExifStatus.Ok }
				}),
				new FakeSelectorStorage(storage),
				_bgTaskQueue, new FakeIWebLogger());

			var actionResult = await controller.PublishCreateAsync("/test.jpg",
				"test", "test", true) as JsonResult;
			var result = actionResult?.Value as string;

			Assert.AreEqual("test", result);
			Assert.IsFalse(storage.ExistFolder(Path.DirectorySeparatorChar + "test"));
			Assert.IsFalse(storage.ExistFile(Path.DirectorySeparatorChar + "test.zip"));
		}

		[TestMethod]
		public void Exist_EmptyString()
		{
			var controller = new PublishController(new AppSettings(), new FakeIPublishPreflight(),
				new FakeIWebHtmlPublishService(),
				new FakeIMetaInfo(new List<FileIndexItem>()),
				new FakeSelectorStorage(),
				_bgTaskQueue, new FakeIWebLogger());
			var actionResult = controller.Exist(string.Empty) as JsonResult;
			var result = actionResult?.Value is bool;
			Assert.IsTrue(result);
		}
	}
}
