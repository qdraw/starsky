using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.Data;
using starsky.Interfaces;
using starsky.Models;
using starsky.Services;
using starsky.ViewModels;
using starskytests.FakeMocks;

namespace starskytests.Controllers
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
            
            var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
            builderDb.UseInMemoryDatabase("test");
            var options = builderDb.Options;
            var contextDb = new ApplicationDbContext(options);
            _query = new Query(contextDb,memoryCache);

            // Create a new http context
//            var context = new DefaultHttpContext();
//            services.AddSingleton<IHttpContextAccessor>(
//                new HttpContextAccessor()
//                {
//                    HttpContext = context,
//                });
            
//            services.AddScoped<IUrlHelper>(x => {
//                var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
//                var factory = x.GetRequiredService<IUrlHelperFactory>();
//                return factory.GetUrlHelper(actionContext);
//            });
            var services = new ServiceCollection();
//            services.AddSingleton<IUrlHelper , FakeUrlHelper>();    

            
        }

        private void InsertSearchData()
        {
            if (string.IsNullOrEmpty(_query.GetItemByHash("home0012304590")))
            {
                _query.AddItem(new FileIndexItem
                {
                    FileName = "hi.jpg",
                    ParentDirectory = "/homecontrollertest",
                    FileHash = "home0012304590",
                    ColorClass = FileIndexItem.Color.Winner // 1
                });
                
                // There must be a parent folder
                _query.AddItem(new FileIndexItem
                {
                    FileName = "homecontrollertest",
                    ParentDirectory = "",
                    IsDirectory = true
                });
            }
        }

        [TestMethod]
        public void HomeControllerIndexDetailViewTest()
        {
            InsertSearchData();
            var controller = new HomeController(_query);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
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
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            var actionResult = controller.Index("/homecontrollertest",null,true) as JsonResult;
            Assert.AreNotEqual(actionResult,null);
            var jsonCollection = actionResult.Value as ArchiveViewModel;
            Assert.AreEqual("home0012304590",jsonCollection.FileIndexItems.FirstOrDefault().FileHash);
        }

        [TestMethod]
        public void HomeControllerIndex404Test()
        {
            var controller = new HomeController(_query);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act
            var actionResult = controller.Index("/not-found-test",null,true) as JsonResult;
            Assert.AreEqual("not found", actionResult.Value);
        }

    }
}