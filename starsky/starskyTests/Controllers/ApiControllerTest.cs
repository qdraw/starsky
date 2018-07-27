using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.Data;
using starsky.Interfaces;
using starsky.Middleware;
using starsky.Models;
using starsky.Services;
using starskytests.Services;

namespace starskytests.Controllers
{
    [TestClass]
    public class ApiControllerTest
    {
        private readonly IQuery _query;
        private readonly IExiftool _exiftool;
        private readonly AppSettings _appSettings;
        private readonly CreateAnImage _createAnImage;

        public ApiControllerTest()
        {
            var provider = new ServiceCollection()
                .AddMemoryCache()
                .BuildServiceProvider();
            var memoryCache = provider.GetService<IMemoryCache>();
            
            var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
            builderDb.UseInMemoryDatabase("test1234");
            var options = builderDb.Options;
            var context = new ApplicationDbContext(options);
            _query = new Query(context,memoryCache);
            
            // Inject Fake Exiftool; dependency injection
            var services = new ServiceCollection();
            services.AddSingleton<IExiftool, FakeExiftool>();    
            
            // Inject Config helper
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            // random config
            _createAnImage = new CreateAnImage();
            var dict = new Dictionary<string, string>
            {
                { "App:StorageFolder", _createAnImage.BasePath },
                { "App:ThumbnailTempFolder",_createAnImage.BasePath },
                { "App:Verbose", "true" }
            };
            // Start using dependency injection
            var builder = new ConfigurationBuilder();  
            // Add random config to dependency injection
            builder.AddInMemoryCollection(dict);
            // build config
            var configuration = builder.Build();
            // inject config as object to a service
            services.ConfigurePoco<AppSettings>(configuration.GetSection("App"));
            // build the service
            var serviceProvider = services.BuildServiceProvider();
            // get the service
            _appSettings = serviceProvider.GetRequiredService<AppSettings>();
           
            // inject exiftool
            _exiftool = serviceProvider.GetRequiredService<IExiftool>();
        }
        
        private FileIndexItem InsertSearchData()
        {
            
            var fileHashCode = FileHash.GetHashCode(_createAnImage.FullFilePath);
            if (string.IsNullOrEmpty(_query.GetItemByHash(fileHashCode)))
            {
                var q = _query.AddItem(new FileIndexItem
                {
                    FileName = _createAnImage.DbPath.Replace("/",string.Empty),
                    ParentDirectory = "/",
                    FileHash = fileHashCode,
                    ColorClass = FileIndexItem.Color.Winner, // 1
                });
            }
            return _query.GetObjectByFilePath(_createAnImage.DbPath);
        }

        [TestMethod]
        public void ApiController_Delete_API_HappyFlow_Test()
        {
            var createAnImage = InsertSearchData();
            var controller = new ApiController(_query,_exiftool,_appSettings);

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
            var controller = new ApiController(_query,_exiftool,_appSettings);
            
            new Thumbnail(_appSettings).CreateThumb(createAnImage);
            
            var actionResult = controller.Thumbnail(createAnImage.FileHash,true,true) as JsonResult;
            Assert.AreNotEqual(actionResult,null);
            var thumbnailAnswer = actionResult.Value as string;
            Assert.AreEqual("OK",thumbnailAnswer);

            var thumbnewImg = new CreateAnImage().BasePath + 
                              Path.DirectorySeparatorChar + createAnImage.FileHash + ".jpg";
            File.Delete(thumbnewImg);
        }
        
        [TestMethod]
        public void ApiController_Thumbnail_HappyFlowFileStreamResult_API_Test()
        {
            var createAnImage = InsertSearchData();
            var controller = new ApiController(_query,_exiftool,_appSettings);
            
            new Thumbnail(_appSettings).CreateThumb(createAnImage);

            var actionResult = controller.Thumbnail(createAnImage.FileHash, true) as FileStreamResult;
            var thumbnailAnswer = actionResult.ContentType;
            Assert.AreEqual("image/jpeg",thumbnailAnswer);
            actionResult.FileStream.Dispose(); // for windows

            var thumbnewImg = new CreateAnImage().BasePath + 
                              Path.DirectorySeparatorChar + createAnImage.FileHash + ".jpg";
            File.Delete(thumbnewImg);
        }

        [TestMethod]
        public void ApiController_Thumbnail_ShowOrginalImage_API_Test()
        {
            var createAnImage = InsertSearchData();
            var controller = new ApiController(_query,_exiftool,_appSettings);

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

            var controller = new ApiController(_query,_exiftool,_appSettings);
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
            var controller = new ApiController(_query,_exiftool,_appSettings);
            var actionResult = controller.Thumbnail(item.FileHash, false, true) as NotFoundObjectResult;
            var thumbnailAnswer = actionResult.StatusCode;
            Assert.AreEqual(404,thumbnailAnswer);
            _query.RemoveItem(item);
        }

        [TestMethod]
        public void ApiController_NonExistingFile_API_Test()
        {
            var controller = new ApiController(_query,_exiftool,_appSettings);
            var actionResult = controller.Thumbnail("404filehash", false, true) as NotFoundObjectResult;
            var thumbnailAnswer = actionResult.StatusCode;
            Assert.AreEqual(404,thumbnailAnswer);
        }

        [TestMethod]
        public void ApiController_starskyTestEnv()
        {
            var controller = new ApiController(_query,_exiftool,_appSettings);
            controller.Env();
        }
        
        [TestMethod]
        public void ApiController_Update_AllDataIncluded_WithFakeExiftool()
        {
            var createAnImage = new CreateAnImage();
            var imageToUpdate = createAnImage.DbPath.Replace("/", string.Empty);
            InsertSearchData();
            
            var controller = new ApiController(_query,_exiftool,_appSettings);
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
            
            var controller = new ApiController(_query,_exiftool,_appSettings);
            var notFoundResult = controller.Update(
                "test", "1", "test", "/345678765434567.jpg") as NotFoundObjectResult;
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
            
            var controller = new ApiController(_query,_exiftool,_appSettings);
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
            
            var controller = new ApiController(_query,_exiftool,_appSettings);
            var notFoundResult = controller.Info("/345678765434567.jpg") as NotFoundObjectResult;
            Assert.AreEqual(404,notFoundResult.StatusCode);
            
            _query.RemoveItem(_query.SingleItem("/345678765434567.jpg").FileIndexItem);
        }
        
        [TestMethod]
        public void ApiController_Delete_SourceImageMissingOnDisk_WithFakeExiftool()
        {
            _query.AddItem(new FileIndexItem
            {
                FileName = "345678765434567.jpg",
                ParentDirectory = "/",
                FileHash = "345678765434567"
            });
            
            var controller = new ApiController(_query,_exiftool,_appSettings);
            var notFoundResult = controller.Delete("/345678765434567.jpg") as NotFoundObjectResult;
            Assert.AreEqual(404,notFoundResult.StatusCode);

            _query.RemoveItem(_query.SingleItem("/345678765434567.jpg").FileIndexItem);
        }
        
        [TestMethod]
        public void ApiController_Thumbnail_NonExistingFile_API_Test()
        {
            var controller = new ApiController(_query,_exiftool,_appSettings);
            var actionResult = controller.Thumbnail("404filehash", false, true) as NotFoundObjectResult;
            var thumbnailAnswer = actionResult.StatusCode;
            Assert.AreEqual(404,thumbnailAnswer);
        }

        [TestMethod]
        public void ApiController_Thumbnail_CorruptImage_NoContentResult_Test()
        {
            // Arrange
            var thumbHash = "ApiController_Thumbnail_CorruptImage_Test";
            var path = _createAnImage.BasePath + Path.DirectorySeparatorChar + thumbHash + ".jpg";
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path)) 
                {
                    sw.WriteLine("CorruptImage");
                } 
            }
            
            _query.AddItem(new FileIndexItem
            {
                FileName = "ApiController_Thumbnail_CorruptImage_Test.jpg",
                ParentDirectory = "/",
                FileHash = thumbHash
            });
            
            // Act
            var controller = new ApiController(_query,_exiftool,_appSettings);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var actionResult = controller.Thumbnail(thumbHash, false, true) as NoContentResult;
            Assert.AreEqual(204,actionResult.StatusCode);
               
            // remove files + database item
            _query.RemoveItem(_query.GetObjectByFilePath("/" + thumbHash + ".jpg"));
            if (File.Exists(path))
            {
                File.Delete(path);
            }

        }
        
        [TestMethod]
        public void ApiController_Thumbnail_CorruptImage_retryThumbnail_Test()
        {
            var thumbHash = "ApiController_Thumbnail_CorruptImage_Test";
            var path = _createAnImage.BasePath + Path.DirectorySeparatorChar + thumbHash + ".jpg";
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path)) 
                {
                    sw.WriteLine("CorruptImage");
                } 
            }
            
            _query.AddItem(new FileIndexItem
            {
                FileName = "ApiController_Thumbnail_CorruptImage_Test.jpg",
                ParentDirectory = "/",
                FileHash = thumbHash
            });
            
            // Act
            var controller = new ApiController(_query,_exiftool,_appSettings);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            // The only difference between ApiController_Thumbnail_CorruptImage_NoContentResult_Test
            var actionResult = controller.Thumbnail(thumbHash, false, true, true) as NotFoundObjectResult;
            Assert.AreEqual(404,actionResult.StatusCode);
               
            // remove files + database item
            _query.RemoveItem(_query.GetObjectByFilePath("/" + thumbHash + ".jpg"));
            if (File.Exists(path))
            {
                File.Delete(path);
            }

        }

        [TestMethod]
        public void ApiController_DownloadPhoto_isThumbnailTrue_CreateThumb_ReturnFileStream_Test()
        {
            // Arange
            var fileIndexItem = InsertSearchData();

            // Remove thumb if exist
            var thumbPath = new CreateAnImage().BasePath + Path.DirectorySeparatorChar + fileIndexItem.FileHash +
                            ".jpg";
            
            if (File.Exists(thumbPath))
            {
                File.Delete(thumbPath);
            }
            
            // Act
            var controller = new ApiController(_query,_exiftool,_appSettings);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            var actionResult =  controller.DownloadPhoto(fileIndexItem.FilePath)  as FileStreamResult;
            Assert.AreNotEqual(null,actionResult);

            actionResult.FileStream.Dispose();

            // Clean
            if (File.Exists(thumbPath))
            {
                File.Delete(thumbPath);
            }
        }
        
        [TestMethod]
        public void ApiController_DownloadPhoto_isThumbnailFalse_ReturnFileStream_Test()
        {
            // Arange
            var fileIndexItem = InsertSearchData();

            // Remove thumb if exist
            var thumbPath = new CreateAnImage().BasePath + 
                            Path.DirectorySeparatorChar + 
                            fileIndexItem.FileHash + ".jpg";
            

            if (File.Exists(thumbPath))
            {
                File.Delete(thumbPath);
            }
            
            // Act
            var controller = new ApiController(_query,_exiftool,_appSettings);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            var actionResult =  controller.DownloadPhoto(fileIndexItem.FilePath,false)  as FileStreamResult;
            Assert.AreNotEqual(null,actionResult);

            actionResult.FileStream.Dispose();


            if (File.Exists(thumbPath))
            {
                File.Delete(thumbPath);
            }
        }
        
        [TestMethod]
        public void ApiController_DownloadPhoto_isThumbnailTrue_ReturnAThumb_ReturnFileStream_Test()
        {
            // Arange
            var fileIndexItem = InsertSearchData();

            // Remove thumb if exist
            var thumbPath = new CreateAnImage().BasePath + Path.DirectorySeparatorChar + fileIndexItem.FileHash +
                            ".jpg";
            
            if (File.Exists(thumbPath))
            {
                File.Delete(thumbPath);
            }
            
            // Act
            var controller = new ApiController(_query,_exiftool,_appSettings);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            // Run once
            var actionResult1 = controller.DownloadPhoto(fileIndexItem.FilePath) as FileStreamResult;
            actionResult1.FileStream.Dispose();


            // Run twice
            var actionResult2 =  controller.DownloadPhoto(fileIndexItem.FilePath)  as FileStreamResult;
            Assert.AreNotEqual(null,actionResult2);

            actionResult2.FileStream.Dispose();

            // Clean
            if (File.Exists(thumbPath))
            {
                File.Delete(thumbPath);
            }
        }

        [TestMethod]
        public void ApiController_DownloadPhoto_SourceImageIsMissing_Test()
        {
            var thumbHash = "ApiController_Thumbnail_CorruptImage_Test";
            var path = _createAnImage.BasePath + Path.DirectorySeparatorChar + thumbHash + ".jpg";
           

            _query.AddItem(new FileIndexItem
            {
                FileName = "ApiController_Thumbnail_CorruptImage_Test.jpg",
                ParentDirectory = "/",
                FileHash = thumbHash
            });

            // Act
            var controller = new ApiController(_query,_exiftool,_appSettings);
            var actionResult =  controller.DownloadPhoto("/" + thumbHash)  as NotFoundObjectResult;
            Assert.AreNotEqual(null,actionResult);
            Assert.AreEqual(404,actionResult.StatusCode);

            
            // remove files + database item
            _query.RemoveItem(_query.GetObjectByFilePath("/" + thumbHash + ".jpg"));
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            
        }
        
        
        [TestMethod]
        public void ApiController_DownloadPhoto_Thumb_base_folder_not_found_Test()
        {
            // this works
            
            // Arange
            var fileIndexItem = InsertSearchData();
            
            var appSettingsthumbtest = _appSettings;
            appSettingsthumbtest.ThumbnailTempFolder = null;
            
            // Act
            var controller = new ApiController(_query,_exiftool,appSettingsthumbtest);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            
            // Run once
            var actionResult = controller.DownloadPhoto(fileIndexItem.FilePath) as NotFoundObjectResult;
            Assert.AreNotEqual(null,actionResult);
            Assert.AreEqual(404,actionResult.StatusCode);

        }

    }
}