using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starskycore.Interfaces;
using starskycore.Services;
using starskycore.ViewModels;

namespace starskytest.Controllers
{
    [TestClass]
    public class SearchControllerTest
    {
        private readonly IQuery _query;
        private readonly ISearch _search;

        public SearchControllerTest()
        {
            var provider = new ServiceCollection()
                .AddMemoryCache()
                .BuildServiceProvider();
            var memoryCache = provider.GetService<IMemoryCache>();

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase(nameof(SearchController));
            var options = builder.Options;
            var context = new ApplicationDbContext(options);
            _query = new Query(context, memoryCache);
            _search = new SearchService(context, memoryCache);
        }

        
        [TestMethod]
        public void SearchControllerTest_ZeroItems_Index()
        {
            var controller = new SearchController(_search);
            var jsonResult = controller.Index("98765456789987",0) as JsonResult;
            var searchViewResult = jsonResult.Value as SearchViewModel;
	        
            Assert.AreEqual(0,searchViewResult.FileIndexItems.Count());
	        Assert.AreEqual("Search",searchViewResult.PageType);

        }

	    [TestMethod]
	    public void SearchControllerTest_Index_OneKeyword()
	    {
		    var item0 = _query.AddItem(new FileIndexItem
		    {
				FileName = "Test.jpg",
			    ParentDirectory = "/",
			    Tags = "test"
		    });
		    var controller = new SearchController(_search);
		    var jsonResult = controller.Index("test",0) as JsonResult;
		    var searchViewResult = jsonResult.Value as SearchViewModel;
		    
		    // some values
		    Assert.AreEqual(1,searchViewResult.SearchCount);
		    Assert.AreEqual(1,searchViewResult.FileIndexItems.Count);
		    Assert.AreEqual(SearchViewModel.SearchForOptionType.Equal,searchViewResult.SearchForOptions[0]);
		    Assert.AreEqual("test",searchViewResult.SearchQuery);
		    Assert.AreEqual(nameof(FileIndexItem.Tags),searchViewResult.SearchIn[0]);

		    _query.RemoveItem(item0);
	    }

	    [TestMethod]
        public void SearchControllerTest_TrashZeroItems()
        {
            var controller = new SearchController(_search);
            var jsonResult = controller.Trash(0) as JsonResult;
            var searchViewResult = jsonResult.Value as SearchViewModel;
            Assert.AreEqual(0,searchViewResult.FileIndexItems.Count());
        }
        
        [TestMethod]
        public void SearchControllerTest_RelativeApi_Prev()
        {
	        var item0 = _query.AddItem(new FileIndexItem
	        {
		        FileName = "test.jpg",
		        ParentDirectory = "/",
		        Tags = "test",
		        FileHash = "FileHash1"
	        });
	        var item1 = _query.AddItem(new FileIndexItem
	        {
		        FileName = "test1.jpg",
		        ParentDirectory = "/",
		        Tags = "test"
	        });
	        var controller = new SearchController(_search);
	        var jsonResult = controller.SearchRelative("/test1.jpg","test", 0) as JsonResult;
	        var relativeObjects = jsonResult.Value as RelativeObjects;
		    
	        // some values
	        Assert.AreEqual("/test.jpg",relativeObjects.PrevFilePath);
	        Assert.AreEqual("FileHash1",relativeObjects.PrevHash);

	        _query.RemoveItem(item0);
	        _query.RemoveItem(item1);
        }
        
        [TestMethod]
        public void SearchControllerTest_RelativeApi_Next()
        {
	        var item0 = _query.AddItem(new FileIndexItem
	        {
		        FileName = "test.jpg",
		        ParentDirectory = "/",
		        Tags = "test",
		        FileHash = "FileHash1"
	        });
	        var item1 = _query.AddItem(new FileIndexItem
	        {
		        FileName = "test1.jpg",
		        ParentDirectory = "/",
		        Tags = "test",
		        FileHash = "FileHash2"
	        });
	        var controller = new SearchController(_search);
	        var jsonResult = controller.SearchRelative("/test.jpg","test", 0) as JsonResult;
	        var relativeObjects = jsonResult.Value as RelativeObjects;
		    
	        // some values
	        Assert.AreEqual("/test1.jpg",relativeObjects.NextFilePath);
	        Assert.AreEqual("FileHash2",relativeObjects.NextHash);

	        _query.RemoveItem(item0);
	        _query.RemoveItem(item1);
        }

        [TestMethod]
        public void SearchRelative_NotFound()
        {
	        var controller = new SearchController(_search);
	        var notFoundObjectResult = controller.SearchRelative("/not-found.jpg","test", 0) as NotFoundObjectResult;

	        Assert.AreEqual(404, notFoundObjectResult.StatusCode);
        }

        [TestMethod]
        public void SearchRelative_LastItem()
        {
	        var item0 = _query.AddItem(new FileIndexItem
	        {
		        FileName = "test.jpg",
		        ParentDirectory = "/",
		        Tags = "test",
		        FileHash = "FileHash1"
	        });
	        var item1 = _query.AddItem(new FileIndexItem
	        {
		        FileName = "test1.jpg",
		        ParentDirectory = "/",
		        Tags = "test",
		        FileHash = "FileHash2"
	        });
	        var controller = new SearchController(_search);
	        var jsonResult = controller.SearchRelative("/test1.jpg","test", 0) as JsonResult;
	        var relativeObjects = jsonResult.Value as RelativeObjects;
		    
	        Assert.IsNull(relativeObjects.NextFilePath);
	        Assert.IsNull(relativeObjects.NextHash);

	        _query.RemoveItem(item0);
	        _query.RemoveItem(item1);
        }

        [TestMethod]
        public void GetIndexFilePathFromSearch_Notfound()
        {
	        var result = new SearchController(_search).GetIndexFilePathFromSearch(new SearchViewModel(),"test");
			Assert.AreEqual(-1, result);
        }
        
        [TestMethod]
        public void RemoveCache_NotFound()
        {
	        var controller = new SearchController(_search);
	        controller.ControllerContext.HttpContext = new DefaultHttpContext();

	        var jsonResult = controller.RemoveCache("non-existing-cache-item") as JsonResult;
	        var resultValue = jsonResult.Value as string;
	        Assert.AreEqual( "there is no cached item", resultValue);
        }
        
        [TestMethod]
        public void RemoveCache_CacheDisabled()
        {
	        var controller = new SearchController(new SearchService(null));
	        controller.ControllerContext.HttpContext = new DefaultHttpContext();

	        var jsonResult = controller.RemoveCache("non-existing-cache-item") as JsonResult;
	        var resultValue = jsonResult.Value as string;
	        Assert.AreEqual( "cache disabled in config", resultValue);
        }
        
        [TestMethod]
        public void RemoveCache_cacheCleared()
        {
	        var controller = new SearchController(_search);
	        controller.ControllerContext.HttpContext = new DefaultHttpContext();

	        _search.Search("1234567890987654");
	        
	        var jsonResult = controller.RemoveCache("1234567890987654") as JsonResult;
	        var resultValue = jsonResult.Value as string;
	        Assert.AreEqual( "cache cleared", resultValue);
        }
    }
}
