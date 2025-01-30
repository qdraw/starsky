using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.feature.import.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public class ImportThumbnailControllerTest
{
	private readonly AppSettings _appSettings;

	public ImportThumbnailControllerTest()
	{
		_appSettings = new AppSettings();
	}

	/// <summary>
	///     Add the file in the underlying request object.
	/// </summary>
	/// <returns>Controller Context with file</returns>
	private static ControllerContext RequestWithFile()
	{
		var httpContext = new DefaultHttpContext();
		httpContext.Request.Headers.Append("Content-Type", "application/octet-stream");
		httpContext.Request.Body = new MemoryStream(CreateAnImage.Bytes.ToArray());

		var actionContext = new ActionContext(httpContext, new RouteData(),
			new ControllerActionDescriptor());
		return new ControllerContext(actionContext);
	}

	[TestMethod]
	public async Task Import_Thumbnail_Ok()
	{
		var services = new ServiceCollection();
		services.AddSingleton<IStorage, FakeIStorage>();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
		var serviceProvider = services.BuildServiceProvider();
		var storageProvider = serviceProvider.GetRequiredService<IStorage>();
		var thumbnailImportService = new ImportThumbnailService(
			new FakeSelectorStorage(storageProvider),
			new FakeIWebLogger(), _appSettings
		);
		var importController =
			new ImportThumbnailController(_appSettings, new FakeSelectorStorage(storageProvider),
				new FakeIThumbnailQuery(), thumbnailImportService)
			{
				ControllerContext = RequestWithFile()
			};
		importController.Request.Headers["filename"] =
			"01234567890123456789123456.jpg"; // len() 26

		var actionResult = await importController.Thumbnail() as JsonResult;
		var list = actionResult?.Value as List<string>;
		var existFileInTempFolder =
			storageProvider.ExistFile(
				_appSettings.TempFolder + "01234567890123456789123456.jpg");

		Assert.AreEqual("01234567890123456789123456", list?.FirstOrDefault());
		Assert.IsFalse(existFileInTempFolder);
	}

	[TestMethod]
	public async Task Import_Thumbnail_Ok_SmallSize()
	{
		var services = new ServiceCollection();
		services.AddSingleton<IStorage, FakeIStorage>();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
		var serviceProvider = services.BuildServiceProvider();
		var storageProvider = serviceProvider.GetRequiredService<IStorage>();
		var thumbnailImportService = new ImportThumbnailService(
			new FakeSelectorStorage(storageProvider),
			new FakeIWebLogger(), _appSettings
		);
		var importController =
			new ImportThumbnailController(_appSettings, new FakeSelectorStorage(storageProvider),
				new FakeIThumbnailQuery(), thumbnailImportService)
			{
				ControllerContext = RequestWithFile()
			};
		importController.Request.Headers["filename"] =
			"01234567890123456789123456@300.jpg"; // len() 26

		var actionResult = await importController.Thumbnail() as JsonResult;
		var list = actionResult?.Value as List<string>;
		var existFileInTempFolder =
			storageProvider.ExistFile(
				_appSettings.TempFolder + "01234567890123456789123456@300.jpg");

		Assert.AreEqual("01234567890123456789123456@300", list?.FirstOrDefault());
		Assert.IsFalse(existFileInTempFolder);
	}

	[TestMethod]
	public async Task Import_Thumbnail_WrongInputName()
	{
		var services = new ServiceCollection();
		services.AddSingleton<IStorage, FakeIStorage>();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();

		var thumbnailImportService = new ImportThumbnailService(
			new FakeSelectorStorage(),
			new FakeIWebLogger(), _appSettings
		);
		var importController =
			new ImportThumbnailController(_appSettings, new FakeSelectorStorage(),
				new FakeIThumbnailQuery(), thumbnailImportService)
			{
				ControllerContext = RequestWithFile()
			};
		importController.Request.Headers["filename"] = "123.jpg"; // len() 3

		var actionResult = await importController.Thumbnail() as JsonResult;
		var list = actionResult?.Value as List<string>;

		Assert.AreEqual(0, list?.Count);
	}

	[TestMethod]
	public async Task Import_Thumbnail_AlreadyExists()
	{
		var services = new ServiceCollection();
		services.AddSingleton<IStorage, FakeIStorage>();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
		var serviceProvider = services.BuildServiceProvider();
		var storageProvider = serviceProvider.GetRequiredService<IStorage>();

		// create already exists
		var empty = new byte[] { 1 }; // new byte[] { } Array.Empty<byte>()
		await storageProvider.WriteStreamAsync(new MemoryStream(empty),
			"91234567890123456789123456");

		var thumbnailImportService = new ImportThumbnailService(
			new FakeSelectorStorage(storageProvider),
			new FakeIWebLogger(), _appSettings
		);
		var importController =
			new ImportThumbnailController(_appSettings, new FakeSelectorStorage(storageProvider),
				new FakeIThumbnailQuery(), thumbnailImportService)
			{
				ControllerContext = RequestWithFile()
			};
		importController.Request.Headers["filename"] =
			"91234567890123456789123456.jpg"; // len() 26

		var actionResult = await importController.Thumbnail() as JsonResult;
		var list = actionResult?.Value as List<string>;
		var existFileInTempFolder =
			storageProvider.ExistFile(
				_appSettings.TempFolder + "91234567890123456789123456.jpg");

		Assert.AreEqual("91234567890123456789123456", list?.FirstOrDefault());
		Assert.IsFalse(existFileInTempFolder);
		Assert.IsTrue(storageProvider.Info("91234567890123456789123456").Size >= 2);
	}
}
