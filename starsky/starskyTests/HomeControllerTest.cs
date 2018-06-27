using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.Data;
using starsky.Interfaces;
using starsky.Models;
using starsky.Services;
using starsky.ViewModels;

namespace starskytests
{
    [TestClass]
    public class HomeControllerTest
    {
        private readonly IQuery _query;

        public HomeControllerTest()
        {
            var provider = new ServiceCollection()
                .AddMemoryCache()
                .BuildServiceProvider();
            var memoryCache = provider.GetService<IMemoryCache>();
            
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase("test");
            var options = builder.Options;
            var context = new ApplicationDbContext(options);
            _query = new Query(context,memoryCache);
        }

        private void InsertSearchData()
        {
            if (string.IsNullOrEmpty(_query.GetItemByHash("home0012304590")))
            {
                _query.AddItem(new FileIndexItem
                {
                    FileName = "hi.jpg",
                    //FilePath = "/homecontrollertest/hi.jpg",
                    ParentDirectory = "/homecontrollertest",
                    FileHash = "home0012304590",
                    ColorClass = FileIndexItem.Color.Winner // 1
                });
            }
        }

        [TestMethod]
        public void HomeControllerIndexDetailViewTest()
        {
            InsertSearchData();
            var controller = new HomeController(_query);
            var actionResult = controller.Index("/homecontrollertest/hi.jpg",null,true) as JsonResult;
            Assert.AreNotEqual(actionResult,null);
            var jsonCollection = actionResult.Value as DetailView;
            Assert.AreEqual("home0012304590",jsonCollection.FileIndexItem.FileHash);
        }

        [TestMethod]
        public void HomeControllerIndexIndexViewModelTest()
        {
            InsertSearchData();
            var controller = new HomeController(_query);
            var actionResult = controller.Index("/homecontrollertest",null,true) as JsonResult;
            Assert.AreNotEqual(actionResult,null);
            var jsonCollection = actionResult.Value as IndexViewModel;
            Assert.AreEqual("home0012304590",jsonCollection.FileIndexItems.FirstOrDefault().FileHash);
        }

//        [TestMethod]
//        public void HomeControllerIndex404Test()
//        {
////            var controller = new HomeController(_query);
//            
//            // Act
////            var actionResult = controller.Index("/not-found-test",null,true) as JsonResult;
////            Assert.AreEqual(404, actionResult.StatusCode);
//  
//        }

    }
}