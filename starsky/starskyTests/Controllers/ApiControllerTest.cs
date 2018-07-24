using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
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
using starskytests.Services;

namespace starskytests.Controllers
{
    [TestClass]
    public class ApiControllerTest
    {
        private readonly IQuery _query;
        private IExiftool _exiftool;

        public ApiControllerTest()
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
            
            // Inject Fake Exiftool; dependency injection
            var services = new ServiceCollection();
            services.AddSingleton<IExiftool, FakeExiftool>();      
            var serviceProvider = services.BuildServiceProvider();
            _exiftool = serviceProvider.GetRequiredService<IExiftool>();
        }
        
        private FileIndexItem InsertSearchData()
        {
            var createAnImage = new CreateAnImage();
            AppSettingsProvider.BasePath = createAnImage.BasePath;
            AppSettingsProvider.ThumbnailTempFolder = createAnImage.BasePath;
            AppSettingsProvider.ReadOnlyFolders = new List<string>();

            Console.WriteLine(createAnImage.BasePath);
            
            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            if (string.IsNullOrEmpty(_query.GetItemByHash(fileHashCode)))
            {
                var q = _query.AddItem(new FileIndexItem
                {
                    FileName = createAnImage.DbPath.Replace("/",string.Empty),
                    ParentDirectory = "/",
                    FileHash = fileHashCode,
                    ColorClass = FileIndexItem.Color.Winner, // 1
                });
            }
            return _query.GetObjectByFilePath(createAnImage.DbPath);
        }

        [TestMethod]
        public void ApiController_Delete_API_HappyFlow_Test()
        {
            var createAnImage = InsertSearchData();
            var controller = new ApiController(_query,_exiftool);

            Console.WriteLine("createAnImage.FilePath");
            Console.WriteLine(createAnImage.FilePath);

            var actionResult = controller.Delete(createAnImage.FilePath) as JsonResult;
            Assert.AreNotEqual(actionResult,null);
            var jsonCollection = actionResult.Value as FileIndexItem;
            Assert.AreEqual(createAnImage.FilePath,jsonCollection.FilePath);
            new CreateAnImage(); //restore afterwards
        }

        [TestMethod]
        public void ApiController_Thumbnail_HappyFlowDisplayJson_API_Test()
        {
            var createAnImage = InsertSearchData();
            var controller = new ApiController(_query,_exiftool);
            
            Thumbnail.CreateThumb(createAnImage);
            
            var actionResult = controller.Thumbnail(createAnImage.FileHash,true,true) as JsonResult;
            Assert.AreNotEqual(actionResult,null);
            var thumbnailAnswer = actionResult.Value as string;
            Assert.AreEqual("OK",thumbnailAnswer);

            var thumbnewImg = new CreateAnImage().BasePath + Path.DirectorySeparatorChar + createAnImage.FileHash + ".jpg";
            File.Delete(thumbnewImg);
        }
        
        [TestMethod]
        public void ApiController_Thumbnail_HappyFlowFileStreamResult_API_Test()
        {
            var createAnImage = InsertSearchData();
            var controller = new ApiController(_query,_exiftool);
            
            Thumbnail.CreateThumb(createAnImage);

            var actionResult = controller.Thumbnail(createAnImage.FileHash, true) as FileStreamResult;
            var thumbnailAnswer = actionResult.ContentType;
            Assert.AreEqual("image/jpeg",thumbnailAnswer);
            actionResult.FileStream.Dispose(); // for windows

            var thumbnewImg = new CreateAnImage().BasePath + Path.DirectorySeparatorChar + createAnImage.FileHash + ".jpg";
            File.Delete(thumbnewImg);
        }

        [TestMethod]
        public void ApiController_Thumbnail_ShowOrginalImage_API_Test()
        {
            var createAnImage = InsertSearchData();
            var controller = new ApiController(_query,_exiftool);

            var actionResult = controller.Thumbnail(createAnImage.FileHash, true) as FileStreamResult;
            var thumbnailAnswer = actionResult.ContentType;
            Assert.AreEqual("image/jpeg",thumbnailAnswer);
            actionResult.FileStream.Dispose(); // for windows

        }

        [TestMethod]
        public void ApiController_ThumbIsMissing_ButOrginalExist_butNoIsSingleItemFlag_API_Test()
        {
            // Photo exist in database but " + "isSingleItem flag is Missing
            var createAnImage = InsertSearchData();

            var controller = new ApiController(_query,_exiftool);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var actionResult = controller.Thumbnail(createAnImage.FileHash, false, true) as NoContentResult;
            var thumbnailAnswer = actionResult.StatusCode;
            Assert.AreNotEqual(200,thumbnailAnswer);
        }

        [TestMethod]
        public void ApiController_FloatingDatabaseFileTest_API_Test()
        {
            var item = _query.AddItem(new FileIndexItem
            {
                //FilePath = "/fakeImage/fake.jpg",
                ParentDirectory = "/fakeImage/",
                FileName = "fake.jpg",
                FileHash = "0986524678765456786543"
            });
            var controller = new ApiController(_query,_exiftool);
            var actionResult = controller.Thumbnail(item.FileHash, false, true) as NotFoundObjectResult;
            var thumbnailAnswer = actionResult.StatusCode;
            Assert.AreEqual(404,thumbnailAnswer);
            _query.RemoveItem(item);
        }

        [TestMethod]
        public void ApiController_NonExistingFile_API_Test()
        {
            var controller = new ApiController(_query,_exiftool);
            var actionResult = controller.Thumbnail("404filehash", false, true) as NotFoundObjectResult;
            var thumbnailAnswer = actionResult.StatusCode;
            Assert.AreEqual(404,thumbnailAnswer);
        }

        [TestMethod]
        public void ApiController_starskyTestEnv()
        {
            var controller = new ApiController(_query,_exiftool);
            controller.Env();
        }
        
        [TestMethod]
        public void ApiController_Update_AllDataIncluded_WithFakeExiftool()
        {
            var createAnImage = new CreateAnImage();
            var imageToUpdate = createAnImage.DbPath.Replace("/", string.Empty);
            InsertSearchData();
            
            var controller = new ApiController(_query,_exiftool);
            var jsonResult = controller.Update("test", "1", "test", createAnImage.DbPath) as JsonResult;
            var exiftoolModel = jsonResult.Value as ExifToolModel;
            Assert.AreEqual("test",exiftoolModel.Tags);            
        }
        
        [TestMethod]
        public void ApiController_Update_SourceImageMissingOnDisk_WithFakeExiftool()
        {
            _query.AddItem(new FileIndexItem
            {
                FileName = "345678765434567.jpg",
                ParentDirectory = "/",
                FileHash = "345678765434567"
            });
            
            var controller = new ApiController(_query,_exiftool);
            var notFoundResult = controller.Update("test", "1", "test", "/345678765434567.jpg") as NotFoundObjectResult;
            Assert.AreEqual(404,notFoundResult.StatusCode);

            _query.RemoveItem(_query.SingleItem("/345678765434567.jpg").FileIndexItem);
        }
        
        [TestMethod]
        public void ApiController_Info_AllDataIncluded_WithFakeExiftool()
        {
            // Using Fake exiftool
            var createAnImage = new CreateAnImage();
            var imageToUpdate = createAnImage.DbPath.Replace("/", string.Empty);
            InsertSearchData();
            
            var controller = new ApiController(_query,_exiftool);
            var jsonResult = controller.Info(createAnImage.DbPath) as JsonResult;
            var exiftoolModel = jsonResult.Value as ExifToolModel;
            Assert.AreEqual(string.Empty,exiftoolModel.Tags);            
        }

        [TestMethod]
        public void ApiController_Info_SourceImageMissingOnDisk_WithFakeExiftool()
        {
            _query.AddItem(new FileIndexItem
            {
                FileName = "345678765434567.jpg",
                ParentDirectory = "/",
                FileHash = "345678765434567"
            });
            
            var controller = new ApiController(_query,_exiftool);
            var notFoundResult = controller.Info("/345678765434567.jpg") as NotFoundObjectResult;
            Assert.AreEqual(404,notFoundResult.StatusCode);
            
            _query.RemoveItem(_query.SingleItem("/345678765434567.jpg").FileIndexItem);
        }

    }
}