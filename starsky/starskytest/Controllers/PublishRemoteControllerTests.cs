using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public class PublishRemoteControllerTests
{
	[TestMethod]
	public async Task PublishFtpAsync_Success()
	{
		var appSettings =
			new AppSettings { TempFolder = Path.DirectorySeparatorChar.ToString() };
		var storage = new FakeIStorage(
			[],
			[Path.DirectorySeparatorChar + "test.zip"]);
		var fakeFtpService = new FakeIFtpService();

		var controller = new PublishRemoteController(new FakeSelectorStorage(storage),
			new FakeIPublishPreflight(), appSettings, fakeFtpService);

		var actionResult = await controller.PublishFtpAsync("test",
			"test", "ftp") as JsonResult;
		var result = actionResult?.Value as bool?;

		Assert.IsTrue(result);
		Assert.AreEqual(Path.DirectorySeparatorChar + "test.zip", fakeFtpService.LastPath);
	}

	[TestMethod]
	public async Task PublishFtpAsync_DisabledForProfile_ReturnsBadRequest()
	{
		var fakeFtpService = new FakeIFtpService();
		var controller = new PublishRemoteController(new FakeSelectorStorage(),
			new FakeIPublishPreflight(isFtpPublishEnabled: false),
			new AppSettings(), fakeFtpService);

		var actionResult =
			await controller.PublishFtpAsync("test", "test", "ftp") as BadRequestObjectResult;

		Assert.AreEqual(400, actionResult?.StatusCode);
	}

	[TestMethod]
	public async Task PublishFtpAsync_RemoteTypeNotFtp_ReturnsBadRequest()
	{
		var appSettings = new AppSettings { TempFolder = Path.DirectorySeparatorChar.ToString() };
		var storage = new FakeIStorage([], [Path.DirectorySeparatorChar + "test.zip"]);
		var fakeFtpService = new FakeIFtpService();
		var controller = new PublishRemoteController(new FakeSelectorStorage(storage),
			new FakeIPublishPreflight(), appSettings, fakeFtpService);

		var actionResult =
			await controller.PublishFtpAsync("test", "test", "s3") as BadRequestObjectResult;
		Assert.AreEqual(400, actionResult?.StatusCode);
		Assert.AreEqual("only ftp is supported", actionResult?.Value);
	}

	[TestMethod]
	public async Task PublishFtpAsync_InvalidModelState_ReturnsBadRequest()
	{
		var appSettings = new AppSettings { TempFolder = Path.DirectorySeparatorChar.ToString() };
		var storage = new FakeIStorage([], [Path.DirectorySeparatorChar + "test.zip"]);
		var fakeFtpService = new FakeIFtpService();
		var controller = new PublishRemoteController(new FakeSelectorStorage(storage),
			new FakeIPublishPreflight(), appSettings, fakeFtpService);
		controller.ModelState.AddModelError("Key", "ErrorMessage");
		var actionResult =
			await controller.PublishFtpAsync("test", "test", "ftp") as BadRequestObjectResult;
		Assert.AreEqual(400, actionResult?.StatusCode);
		Assert.AreEqual("Model invalid", actionResult?.Value);
	}

	[TestMethod]
	public async Task PublishFtpAsync_ProfileInvalid_ReturnsBadRequest()
	{
		var appSettings = new AppSettings { TempFolder = Path.DirectorySeparatorChar.ToString() };
		var storage = new FakeIStorage([], [Path.DirectorySeparatorChar + "test.zip"]);
		var fakeFtpService = new FakeIFtpService();
		// Use isOk = false to simulate invalid profile
		var controller = new PublishRemoteController(new FakeSelectorStorage(storage),
			new FakeIPublishPreflight(isOk: false), appSettings, fakeFtpService);
		var actionResult =
			await controller.PublishFtpAsync("test", "test", "ftp") as BadRequestObjectResult;
		Assert.AreEqual(400, actionResult?.StatusCode);
	}

	[TestMethod]
	public async Task PublishFtpAsync_FtpUploadFailed_ReturnsBadRequest()
	{
		var appSettings = new AppSettings { TempFolder = Path.DirectorySeparatorChar.ToString() };
		var storage = new FakeIStorage([], [Path.DirectorySeparatorChar + "test.zip"]);
		var fakeFtpService = new FakeIFtpService { RunResult = false };
		var controller = new PublishRemoteController(new FakeSelectorStorage(storage),
			new FakeIPublishPreflight(), appSettings, fakeFtpService);
		var actionResult =
			await controller.PublishFtpAsync("test", "test", "ftp") as BadRequestObjectResult;
		Assert.AreEqual(400, actionResult?.StatusCode);
		Assert.AreEqual("FTP upload failed", actionResult?.Value);
	}

	[TestMethod]
	public async Task PublishFtpAsync_ManifestNull_ReturnsBadRequest()
	{
		var appSettings = new AppSettings { TempFolder = Path.DirectorySeparatorChar.ToString() };
		var storage = new FakeIStorage([], [Path.DirectorySeparatorChar + "test.zip"]);
		var fakeFtpService = new FakeIFtpService { ManifestResult = null };
		var controller = new PublishRemoteController(new FakeSelectorStorage(storage),
			new FakeIPublishPreflight(), appSettings, fakeFtpService);
		var actionResult =
			await controller.PublishFtpAsync("test", "test", "ftp") as BadRequestObjectResult;
		Assert.AreEqual(400, actionResult?.StatusCode);
		Assert.AreEqual("Publish zip is invalid", actionResult?.Value);
	}

	[TestMethod]
	public async Task PublishFtpAsync_ZipNotFound_ReturnsNotFound_Remote()
	{
		var appSettings = new AppSettings { TempFolder = Path.DirectorySeparatorChar.ToString() };
		var storage = new FakeIStorage([], []);
		var fakeFtpService = new FakeIFtpService();
		var controller = new PublishRemoteController(new FakeSelectorStorage(storage),
			new FakeIPublishPreflight(), appSettings, fakeFtpService);
		var actionResult =
			await controller.PublishFtpAsync("test", "test", "ftp") as NotFoundObjectResult;
		Assert.AreEqual(404, actionResult?.StatusCode);
		Assert.AreEqual("Publish zip not found", actionResult?.Value);
	}
}
