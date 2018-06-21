﻿using Microsoft.AspNetCore.Mvc;
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
            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            if (string.IsNullOrEmpty(_query.GetItemByHash(fileHashCode)))
            {
                _query.AddItem(new FileIndexItem
                {
                    FileName = createAnImage.DbPath.Replace("/",string.Empty),
                    FilePath = createAnImage.DbPath,
                    ParentDirectory = "/",
                    FileHash = fileHashCode,
                    ColorClass = FileIndexItem.Color.Winner // 1
                });
            }
            return _query.GetObjectByFilePath(createAnImage.DbPath);
        }

        [TestMethod]
        public void ApiController_Delete_API_Test()
        {
            var createAnImage = InsertSearchData();
            var controller = new ApiController(_query);

            var actionResult = controller.Delete(createAnImage.FilePath) as JsonResult;
            Assert.AreNotEqual(actionResult,null);
            var jsonCollection = actionResult.Value as FileIndexItem;
            Assert.AreEqual(createAnImage.FilePath,jsonCollection.FilePath);
            new CreateAnImage(); //restore afterwards
        }

    }
}