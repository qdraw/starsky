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
		var fakePublishService = new FakeIRemotePublishService();

		var controller = new PublishRemoteController(new FakeSelectorStorage(storage),
			new FakeIPublishPreflight(), appSettings, fakePublishService);

		var actionResult = await controller.PublishRemoteAsync("test",
			"test") as JsonResult;
		var result = actionResult?.Value as bool?;

		Assert.IsTrue(result);
		Assert.AreEqual(Path.DirectorySeparatorChar + "test.zip", fakePublishService.LastPath);
	}

	[TestMethod]
	public async Task PublishFtpAsync_InvalidModelState_ReturnsBadRequest()
	{
		var appSettings = new AppSettings { TempFolder = Path.DirectorySeparatorChar.ToString() };
		var storage = new FakeIStorage([], [Path.DirectorySeparatorChar + "test.zip"]);
		var fakePublishSelector = new FakeIRemotePublishService();
		var controller = new PublishRemoteController(new FakeSelectorStorage(storage),
			new FakeIPublishPreflight(), appSettings, fakePublishSelector);
		controller.ModelState.AddModelError("Key", "ErrorMessage");
		var actionResult =
			await controller.PublishRemoteAsync("test", "test") as BadRequestObjectResult;
		Assert.AreEqual(400, actionResult?.StatusCode);
		Assert.AreEqual("Model invalid", actionResult?.Value);
	}

	[TestMethod]
	public async Task PublishFtpAsync_ProfileInvalid_ReturnsBadRequest()
	{
		var appSettings = new AppSettings { TempFolder = Path.DirectorySeparatorChar.ToString() };
		var storage = new FakeIStorage([], [Path.DirectorySeparatorChar + "test.zip"]);
		var fakePublishSelector = new FakeIRemotePublishService();
		// Use isOk = false to simulate invalid profile
		var controller = new PublishRemoteController(new FakeSelectorStorage(storage),
			new FakeIPublishPreflight(isOk: false), appSettings, fakePublishSelector);
		var actionResult =
			await controller.PublishRemoteAsync("test", "test") as BadRequestObjectResult;
		Assert.AreEqual(400, actionResult?.StatusCode);
	}

	[TestMethod]
	public async Task PublishFtpAsync_FtpUploadFailed_ReturnsBadRequest()
	{
		var appSettings = new AppSettings { TempFolder = Path.DirectorySeparatorChar.ToString() };
		var storage = new FakeIStorage([], [Path.DirectorySeparatorChar + "test.zip"]);
		var fakePublishSelector = new FakeIRemotePublishService { RunResult = false };
		var controller = new PublishRemoteController(new FakeSelectorStorage(storage),
			new FakeIPublishPreflight(), appSettings, fakePublishSelector);
		var actionResult =
			await controller.PublishRemoteAsync("test", "test") as BadRequestObjectResult;
		Assert.AreEqual(400, actionResult?.StatusCode);
		Assert.Contains("failed", actionResult?.Value as string ?? string.Empty);
	}

	[TestMethod]
	public async Task PublishFtpAsync_ManifestNull_ReturnsBadRequest()
	{
		var appSettings = new AppSettings { TempFolder = Path.DirectorySeparatorChar.ToString() };
		var storage = new FakeIStorage([], [Path.DirectorySeparatorChar + "test.zip"]);
		var fakePublishSelector = new FakeIRemotePublishService { ManifestResult = null };
		var controller = new PublishRemoteController(new FakeSelectorStorage(storage),
			new FakeIPublishPreflight(), appSettings, fakePublishSelector);
		var actionResult =
			await controller.PublishRemoteAsync("test", "test") as BadRequestObjectResult;
		Assert.AreEqual(400, actionResult?.StatusCode);
		Assert.AreEqual("Publish zip is invalid", actionResult?.Value);
	}

	[TestMethod]
	public async Task PublishFtpAsync_ZipNotFound_ReturnsNotFound_Remote()
	{
		var appSettings = new AppSettings { TempFolder = Path.DirectorySeparatorChar.ToString() };
		var storage = new FakeIStorage([], []);
		var fakePublishSelector = new FakeIRemotePublishService();
		var controller = new PublishRemoteController(new FakeSelectorStorage(storage),
			new FakeIPublishPreflight(), appSettings, fakePublishSelector);
		var actionResult =
			await controller.PublishRemoteAsync("test", "test") as NotFoundObjectResult;
		Assert.AreEqual(404, actionResult?.StatusCode);
		Assert.AreEqual("Publish zip not found", actionResult?.Value);
	}

	[TestMethod]
	public async Task PublishFtpAsync_LocalFileSystem_Success()
	{
		var appSettings =
			new AppSettings { TempFolder = Path.DirectorySeparatorChar.ToString() };
		var storage = new FakeIStorage(
			[],
			[Path.DirectorySeparatorChar + "test.zip"]);
		var fakePublishSelector = new FakeIRemotePublishService();

		var controller = new PublishRemoteController(new FakeSelectorStorage(storage),
			new FakeIPublishPreflight(), appSettings, fakePublishSelector);

		var actionResult = await controller.PublishRemoteAsync("test",
			"test") as JsonResult;
		var result = actionResult?.Value as bool?;

		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task PublishFtpAsync_PublishDisabled_ReturnsBadRequest()
	{
		var appSettings = new AppSettings { TempFolder = Path.DirectorySeparatorChar.ToString() };
		var storage = new FakeIStorage([], [Path.DirectorySeparatorChar + "test.zip"]);
		var fakePublishSelector = new FakeIRemotePublishService(false);
		var controller = new PublishRemoteController(new FakeSelectorStorage(storage),
			new FakeIPublishPreflight(), appSettings, fakePublishSelector);
		var actionResult =
			await controller.PublishRemoteAsync("test", "test") as BadRequestObjectResult;
		Assert.AreEqual(400, actionResult?.StatusCode);
		Assert.AreEqual("FTP publishing disabled for publish profile", actionResult?.Value);
	}

	[TestMethod]
	public void Status_ValidProfile_PublishEnabled_ReturnsJsonFalse()
	{
		var appSettings = new AppSettings();
		var storage = new FakeIStorage([], []);
		var fakePublishPreflight = new FakeIPublishPreflight(isOk: true);
		var fakePublishService = new FakeIRemotePublishService(true);
		var controller = new PublishRemoteController(new FakeSelectorStorage(storage),
			fakePublishPreflight, appSettings, fakePublishService);
		var actionResult = controller.Status("test") as JsonResult;
		Assert.IsNotNull(actionResult);
		Assert.IsFalse(( bool? )  actionResult.Value );
	}

	[TestMethod]
	public void Status_ValidProfile_PublishDisabled_ReturnsJsonTrue()
	{
		var appSettings = new AppSettings();
		var storage = new FakeIStorage([], []);
		var fakePublishPreflight = new FakeIPublishPreflight(isOk: true);
		var fakePublishService = new FakeIRemotePublishService(false);
		var controller = new PublishRemoteController(new FakeSelectorStorage(storage),
			fakePublishPreflight, appSettings, fakePublishService);
		var actionResult = controller.Status("test") as JsonResult;
		Assert.IsNotNull(actionResult);
		Assert.IsTrue(( bool? )  actionResult.Value );
	}

	[TestMethod]
	public void Status_InvalidProfile_ReturnsJsonTrue()
	{
		var appSettings = new AppSettings();
		var storage = new FakeIStorage([], []);
		var fakePublishPreflight = new FakeIPublishPreflight(isOk: false);
		var fakePublishService = new FakeIRemotePublishService(true);
		var controller = new PublishRemoteController(new FakeSelectorStorage(storage),
			fakePublishPreflight, appSettings, fakePublishService);
		var actionResult = controller.Status("test") as JsonResult;
		Assert.IsNotNull(actionResult);
		Assert.IsTrue(( bool? )  actionResult.Value );
	}
}
