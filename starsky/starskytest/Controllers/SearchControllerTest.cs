using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starskycore.Data;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;
using starskycore.ViewModels;
using Query = starskycore.Services.Query;

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
            _search = new SearchService(context);
        }

        [TestMethod]
        public void SearchControllerTest_IndexPost()
        {
            var controller = new SearchController(_search);
            var redirectToActionResult = controller.IndexPost("98765456789987") as RedirectToActionResult;
            Assert.AreEqual("Index",redirectToActionResult.ActionName);
        }
        
        [TestMethod]
        public void SearchControllerTest_ZeroItems_Index()
        {
            var controller = new SearchController(_search);
            var jsonResult = controller.Index("98765456789987",0,true) as JsonResult;
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
		    var jsonResult = controller.Index("test",0,true) as JsonResult;
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
            var jsonResult = controller.Trash(0,true) as JsonResult;
            var searchViewResult = jsonResult.Value as SearchViewModel;
            Assert.AreEqual(0,searchViewResult.FileIndexItems.Count());
        }
        
    }
}