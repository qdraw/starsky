using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.project.web.ViewModels;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public sealed class IndexControllerTest
{
	private readonly Query _query;

	public IndexControllerTest()
	{
		var provider = new ServiceCollection()
			.AddMemoryCache()
			.BuildServiceProvider();
		var memoryCache = provider.GetService<IMemoryCache>();

		var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
		builderDb.UseInMemoryDatabase("test");
		var options = builderDb.Options;
		var contextDb = new ApplicationDbContext(options);
		_query = new Query(contextDb, new AppSettings(), null!,
			new FakeIWebLogger(), memoryCache);
	}

	private async Task InsertSearchData()
	{
		var fileSubPath =
			( await _query.GetSubPathsByHashAsync("home0012304590") ).FirstOrDefault();
		if ( string.IsNullOrEmpty(fileSubPath) )
		{
			await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "hi.jpg",
				ParentDirectory = "/homecontrollertest",
				FileHash = "home0012304590",
				ColorClass = ColorClassParser.Color.Winner // 1
			});

			// There must be a parent folder
			await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "homecontrollertest", ParentDirectory = "", IsDirectory = true
			});
		}
	}

	[TestMethod]
	public async Task HomeControllerIndexDetailViewTest()
	{
		await InsertSearchData();
		var controller = new IndexController(_query, new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		var actionResult = controller.Index("/homecontrollertest/hi.jpg") as JsonResult;
		Assert.IsNotNull(actionResult);
		var jsonCollection = actionResult.Value as DetailView;
		Assert.AreEqual("home0012304590", jsonCollection?.FileIndexItem?.FileHash);
	}

	[TestMethod]
	public async Task HomeControllerIndexIndexViewModelTest()
	{
		await InsertSearchData();
		var controller = new IndexController(_query, new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		var actionResult = controller.Index("/homecontrollertest") as JsonResult;
		Assert.IsNotNull(actionResult);
		var jsonCollection = actionResult.Value as ArchiveViewModel;
		Assert.AreEqual("home0012304590", jsonCollection?
			.FileIndexItems.FirstOrDefault()?.FileHash);
	}

	[TestMethod]
	[SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
	public void HomeControllerIndexIndexViewModel_SlashPage_Test()
	{
		var fakeQuery = new FakeIQuery(new List<FileIndexItem>
		{
			new("/") { IsDirectory = true },
			new("/test.jpg") { Tags = "test", FileHash = "test" }
		});

		var controller = new IndexController(fakeQuery, new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		var actionResult = controller.Index("/") as JsonResult;
		Assert.IsNotNull(actionResult);
		var jsonCollection = actionResult.Value as ArchiveViewModel;
		Assert.AreEqual("test", jsonCollection?.FileIndexItems.FirstOrDefault()?.FileHash);
	}

	[TestMethod]
	public void HomeControllerIndexIndexViewModel_EmptyStringPage_Test()
	{
		var fakeQuery = new FakeIQuery(new List<FileIndexItem>
		{
			new("/") { IsDirectory = true },
			new("/test.jpg") { Tags = "test", FileHash = "test" }
		});

		var controller = new IndexController(fakeQuery, new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		var actionResult = controller.Index(string.Empty) as JsonResult;
		Assert.IsNotNull(actionResult);
		var jsonCollection = actionResult.Value as ArchiveViewModel;
		Assert.AreEqual("test", jsonCollection?.FileIndexItems.FirstOrDefault()?.FileHash);
	}

	[TestMethod]
	public void Index_RootRequest_WithTenantSlug_MapsToTenantRoot()
	{
		var fakeQuery = new FakeIQuery(new List<FileIndexItem>
		{
			new("/main") { IsDirectory = true },
			new("/main/root-file.jpg")
			{
				ParentDirectory = "/main",
				FileHash = "tenant-root-file",
				IsDirectory = false
			}
		});

		var controller = new IndexController(fakeQuery, new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		controller.ControllerContext.HttpContext.Items[TenantConstants.TenantSlugItemKey] = "main";

		var actionResult = controller.Index("/") as JsonResult;
		Assert.IsNotNull(actionResult);
		var jsonCollection = actionResult.Value as ArchiveViewModel;
		Assert.IsNotNull(jsonCollection);
		Assert.AreEqual("/main", jsonCollection.SubPath);
		Assert.AreEqual("tenant-root-file", jsonCollection.FileIndexItems.FirstOrDefault()?.FileHash);
	}

	[TestMethod]
	public void Index_ChildRequest_WithTenantSlug_MapsToTenantChildPath()
	{
		var fakeQuery = new FakeIQuery(new List<FileIndexItem>
		{
			new("/main") { IsDirectory = true },
			new("/main/0001") { IsDirectory = true },
			new("/main/0001/child-file.jpg")
			{
				ParentDirectory = "/main/0001",
				FileHash = "tenant-child-file",
				IsDirectory = false
			}
		});

		var controller = new IndexController(fakeQuery, new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		controller.ControllerContext.HttpContext.Items[TenantConstants.TenantSlugItemKey] = "main";

		var actionResult = controller.Index("/0001") as JsonResult;
		Assert.IsNotNull(actionResult);
		var jsonCollection = actionResult.Value as ArchiveViewModel;
		Assert.IsNotNull(jsonCollection);
		Assert.AreEqual("/main/0001", jsonCollection.SubPath);
		Assert.AreEqual("tenant-child-file", jsonCollection.FileIndexItems.FirstOrDefault()?.FileHash);
	}

	[TestMethod]
	public void Index_BadRequest()
	{
		var controller = new IndexController(new FakeIQuery(), new AppSettings());

		controller.ModelState.AddModelError("Key", "ErrorMessage");

		var result = controller.Index(null!);

		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}

	[TestMethod]
	public void HomeControllerIndex404Test()
	{
		var controller = new IndexController(_query, new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		// Act
		var actionResult = controller.Index("/not-found-test") as JsonResult;
		Assert.AreEqual("not found", actionResult?.Value);
	}

	[TestMethod]
	public async Task Index_NoItem_Give_Success_OnHome()
	{
		await InsertSearchData();
		var controller = new IndexController(new FakeIQuery(), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		var actionResult = controller.Index() as JsonResult;
		Assert.IsNotNull(actionResult);
		var jsonCollection = actionResult.Value as ArchiveViewModel;
		Assert.AreEqual(0, jsonCollection?.FileIndexItems.Count());
	}
}
