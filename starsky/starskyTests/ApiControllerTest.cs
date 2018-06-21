using System.Collections.Generic;
using System.IO;
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
    public class ApiControllerTest
    {
        private readonly IQuery _query;

        public ApiControllerTest()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase("test");
            var options = builder.Options;
            var context = new ApplicationDbContext(options);
            _query = new Query(context);
        }
        
        private FileIndexItem InsertSearchData()
        {
            var createAnImage = new CreateAnImage();
            AppSettingsProvider.BasePath = createAnImage.BasePath;
            AppSettingsProvider.ThumbnailTempFolder = createAnImage.BasePath;
            AppSettingsProvider.ReadOnlyFolders = new List<string>();
            
            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            if (string.IsNullOrEmpty(_query.GetItemByHash(fileHashCode)))
            {
                _query.AddItem(new FileIndexItem
                {
                    FileName = createAnImage.DbPath.Replace("/",string.Empty),
                    FilePath = createAnImage.DbPath,
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
            var controller = new ApiController(_query);

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
            var controller = new ApiController(_query);
            
            Thumbnail.CreateThumb(createAnImage);
            
            var actionResult = controller.Thumbnail(createAnImage.FileHash,true,true) as JsonResult;
            Assert.AreNotEqual(actionResult,null);
            var thumbnailAnswer = actionResult.Value as string;
            Assert.AreEqual("OK",thumbnailAnswer);

            var thumbnewImg = new CreateAnImage().BasePath + Path.DirectorySeparatorChar + createAnImage.FileHash + ".jpg";
            File.Delete(thumbnewImg);
        }

        [TestMethod]
        public void ApiController_Thumbnail_ShowOrginalImage_API_Test()
        {
            var createAnImage = InsertSearchData();
            var controller = new ApiController(_query);

            var actionResult = controller.Thumbnail(createAnImage.FileHash, true, true) as FileStreamResult;
            var thumbnailAnswer = actionResult.ContentType;
            Assert.AreEqual("image/jpeg",thumbnailAnswer);
        }

    }
}