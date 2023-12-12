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
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Controllers
{
	[TestClass]
	public class ImportThumbnailControllerTest
	{
		private readonly AppSettings _appSettings;

		public ImportThumbnailControllerTest()
		{
			_appSettings = new AppSettings();
		}
		
		/// <summary>
		///  Add the file in the underlying request object.
		/// </summary>
		/// <returns>Controller Context with file</returns>
		private static ControllerContext RequestWithFile()
		{
			var httpContext = new DefaultHttpContext();
			httpContext.Request.Headers.Add("Content-Type", "application/octet-stream");
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
			var importController = new ImportThumbnailController(_appSettings, 
				new FakeSelectorStorage(),new FakeIWebLogger(), 
				new FakeIThumbnailQuery())
			{
				ControllerContext = RequestWithFile(),
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
			var importController = new ImportThumbnailController(_appSettings, 
				new FakeSelectorStorage(),new FakeIWebLogger(), 
				new FakeIThumbnailQuery())
			{
				ControllerContext = RequestWithFile(),
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

			var importController = new ImportThumbnailController(_appSettings, 
				new FakeSelectorStorage(),new FakeIWebLogger(), 
				new FakeIThumbnailQuery())
			{
				ControllerContext = RequestWithFile(),
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
			var empty = new byte[] {1}; // new byte[] { } Array.Empty<byte>()
			await storageProvider.WriteStreamAsync(new MemoryStream(empty),
				"91234567890123456789123456");
			
			var importController = new ImportThumbnailController(_appSettings, 
				new FakeSelectorStorage(storageProvider),new FakeIWebLogger(), 
				new FakeIThumbnailQuery())
			{
				ControllerContext = RequestWithFile(),
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

		[TestMethod]
		public async Task WriteThumbnailsTest_ListShouldBeEq()
		{
			var service = new ImportThumbnailController(_appSettings,
				new FakeSelectorStorage(), new FakeIWebLogger(), new FakeIThumbnailQuery());
			var result = await service.WriteThumbnails(new List<string>(), new List<string>{ "123" });
			Assert.IsFalse(result);
		}
		
		[TestMethod]
		public async Task WriteThumbnailsTest_NotFound()
		{
			var logger = new FakeIWebLogger();
			var service = new ImportThumbnailController(_appSettings,
				new FakeSelectorStorage(), logger, new FakeIThumbnailQuery());
			var result = await service.WriteThumbnails(new List<string>{ "123" }, new List<string>{ "123" });
			
			Assert.IsTrue(result);
			Assert.IsTrue(logger.TrackedInformation.FirstOrDefault().Item2.Contains("not exist"));
		}
		
		[TestMethod]
		public async Task WriteThumbnailsTest_ShouldMoveFile()
		{
			var logger = new FakeIWebLogger();
			var storage = new FakeIStorage(new List<string>(), new List<string>{ "/upload/123.jpg" });
			var service = new ImportThumbnailController(_appSettings,
				new FakeSelectorStorage(storage), logger, new FakeIThumbnailQuery());
			await service.WriteThumbnails(new List<string>{  "/upload/123.jpg" }, new List<string>{ "123" });
			
			Assert.IsFalse(storage.ExistFile("/upload/123.jpg"));
			Assert.IsTrue(storage.ExistFile("123"));
		}
		
		
		[TestMethod]
		public void MapToTransferObject1()
		{
			var inputList = new List<string> { "12345678901234567890123456" };
			var result = ImportThumbnailController.MapToTransferObject(inputList);
			Assert.AreEqual("12345678901234567890123456", result.FirstOrDefault().FileHash);
			Assert.AreEqual(true, result.FirstOrDefault().Large);
		}
		
		[TestMethod]
		public void MapToTransferObject1_2000()
		{
			var inputList = new List<string> { "12345678901234567890123456@2000" };
			var result = ImportThumbnailController.MapToTransferObject(inputList);
			Assert.AreEqual("12345678901234567890123456", result.FirstOrDefault().FileHash);
			Assert.AreEqual(true, result.FirstOrDefault().ExtraLarge);
		}
		
		[TestMethod]
		[ExpectedException(typeof(System.ArgumentOutOfRangeException))]
		public void MapToTransferObject1_NonValidType()
		{
			var inputList = new List<string> { "1234567890123456" };
			ImportThumbnailController.MapToTransferObject(inputList);
		}
	}
}
