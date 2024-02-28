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

namespace starskytest.Controllers
{
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
			if ( string.IsNullOrEmpty(await _query.GetSubPathByHashAsync("home0012304590")) )
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
			Assert.AreNotEqual(null, actionResult);
			var jsonCollection = actionResult?.Value as DetailView;
			Assert.AreEqual("home0012304590", jsonCollection?.FileIndexItem?.FileHash);
		}

		[TestMethod]
		public async Task HomeControllerIndexIndexViewModelTest()
		{
			await InsertSearchData();
			var controller = new IndexController(_query, new AppSettings());
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			var actionResult = controller.Index("/homecontrollertest") as JsonResult;
			Assert.AreNotEqual(null, actionResult);
			var jsonCollection = actionResult?.Value as ArchiveViewModel;
			Assert.AreEqual("home0012304590", jsonCollection?
				.FileIndexItems.FirstOrDefault()?.FileHash);
		}

		[TestMethod]
		[SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
		public void HomeControllerIndexIndexViewModel_SlashPage_Test()
		{
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/") { IsDirectory = true },
				new FileIndexItem("/test.jpg") { Tags = "test", FileHash = "test" }
			});

			var controller = new IndexController(fakeQuery, new AppSettings());
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			var actionResult = controller.Index("/") as JsonResult;
			Assert.AreNotEqual(null, actionResult);
			var jsonCollection = actionResult?.Value as ArchiveViewModel;
			Assert.AreEqual("test", jsonCollection?.FileIndexItems.FirstOrDefault()?.FileHash);
		}

		[TestMethod]
		public void HomeControllerIndexIndexViewModel_EmptyStringPage_Test()
		{
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/") { IsDirectory = true },
				new FileIndexItem("/test.jpg") { Tags = "test", FileHash = "test" }
			});

			var controller = new IndexController(fakeQuery, new AppSettings());
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			var actionResult = controller.Index(string.Empty) as JsonResult;
			Assert.AreNotEqual(null, actionResult);
			var jsonCollection = actionResult?.Value as ArchiveViewModel;
			Assert.AreEqual("test", jsonCollection?.FileIndexItems.FirstOrDefault()?.FileHash);
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
			Assert.AreNotEqual(null, actionResult);
			var jsonCollection = actionResult?.Value as ArchiveViewModel;
			Assert.AreEqual(0, jsonCollection?.FileIndexItems.Count());
		}
	}
}
