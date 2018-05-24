using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase("test");
            var options = builder.Options;
            var context = new ApplicationDbContext(options);
            _query = new Query(context);
        }
        
        [TestMethod]
        public void HomeControllerIndexTest()
        {
            _query.AddItem(new FileIndexItem
            {
                FileName = "hi.jpg",
                FilePath = "/homecontrollertest/hi.jpg",
                ParentDirectory = "/homecontrollertest",
                FileHash = "home0012304590",
                ColorClass = FileIndexItem.Color.Winner // 1
            });
            var controller = new HomeController(_query);
            var actionResult = controller.Index("/homecontrollertest",null,true) as JsonResult;
            var jsonCollection = actionResult.Value as IndexViewModel;
                        
            Assert.AreEqual("home0012304590",jsonCollection.FileIndexItems.FirstOrDefault().FileHash);
        }

    }
}