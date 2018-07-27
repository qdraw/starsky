//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Caching.Memory;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using starsky.Controllers;
//using starsky.Data;
//using starsky.Interfaces;
//using starsky.Services;
//using starskytests.Services;
//
//namespace starskytests.Controllers
//{
//    [TestClass]
//    public class ImportControllerTest
//    {
//        private readonly ImportService _import;
//        private readonly ImportController _importController;
//        private IExiftool _exiftool;
//
//        public ImportControllerTest()
//        {
//            var provider = new ServiceCollection()
//                .AddMemoryCache()
//                .BuildServiceProvider();
//            
//            var memoryCache = provider.GetService<IMemoryCache>();
//
//            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
//            builder.UseInMemoryDatabase("test");
//            var options = builder.Options;
//            var context = new ApplicationDbContext(options);
//            IQuery query = new Query(context,memoryCache);
//            var isync = new SyncService(context,query);
//            
//            // Inject Fake Exiftool; dependency injection
//            var services = new ServiceCollection();
//            services.AddSingleton<IExiftool, FakeExiftool>();      
//            var serviceProvider = services.BuildServiceProvider();
//            _exiftool = serviceProvider.GetRequiredService<IExiftool>();
//
//            
//            _import = new ImportService(context, isync, _exiftool);
//            _importController = new ImportController(_import);
//        }
//
////        [TestMethod]
////        public void TEst()
////        {
////            _exiftool.Info("test");
////            
////        }
//        
////        [TestMethod]
////        public async Task ImportController_IndexPost()
////        {
////            // todo: test is not good
////            AppSettingsProvider.ThumbnailTempFolder = new CreateAnImage().BasePath;
////            var index = await _importController.IndexPost() as JsonResult;
////            Assert.AreNotEqual(500,index.StatusCode);
////        }
//      
//
//    }
//}