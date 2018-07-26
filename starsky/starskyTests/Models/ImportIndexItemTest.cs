//using System;
//using System.Globalization;
//using System.IO;
//using System.Text.RegularExpressions;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using starsky.Models;
//
//namespace starskytests.Models
//{
//    [TestClass]
//    public class ImportIndexItemTest
//    {
//        [TestMethod]
//        public void ImportIndexItemParseFileNameTest()
//        {
//            var createAnImage = new CreateAnImage();
//            AppSettingsProvider.Structure = "/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss.ext";
//
//            var importItem = new ImportIndexItem();
//            importItem.SourceFullFilePath = createAnImage.FullFilePath;
//
//            var fileName = importItem.ParseFileName();
//            Assert.AreEqual("00010101_000000.jpg", fileName);
//        }
//
//        [TestMethod]
//        public void ImportIndexItemParseSubfoldersTest()
//        {
//            var createAnImage = new CreateAnImage();
//            var importItem = new ImportIndexItem();
//            importItem.SourceFullFilePath = createAnImage.FullFilePath;
//            AppSettingsProvider.Structure = "/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss.ext";
//            AppSettingsProvider.BasePath = createAnImage.BasePath;
//            var s = importItem.ParseSubfolders(false);
//            Assert.AreEqual("/0001/01/0001_01_01/",s);
//        }
//
//        [TestMethod]
//        public void ImportIndexItemParseSubfolders_TRslashABC_Test()
//        {
//            var createAnImage = new CreateAnImage();
//            var importItem = new ImportIndexItem();
//            AppSettingsProvider.Structure = "/\\t\\r/\\a\\b\\c/test";
//
//            importItem.SourceFullFilePath = createAnImage.FullFilePath;
//            AppSettingsProvider.BasePath = createAnImage.BasePath;
//            var s = importItem.ParseSubfolders(false);
//            Assert.AreEqual("/tr/abc/",s);
//        }
//        
//        [TestMethod]
//        public void ImportIndexItemParseSubfolders_Tzzz_slashABC_Test()
//        {
//            var createAnImage = new CreateAnImage();
//            var importItem = new ImportIndexItem();
//            AppSettingsProvider.Structure = "/\\t\\z/\\a\\b\\c/test";
//
//            importItem.SourceFullFilePath = createAnImage.FullFilePath;
//            AppSettingsProvider.BasePath = createAnImage.BasePath;
//            var s = importItem.ParseSubfolders(false);
//            Assert.AreEqual("/tz/abc/",s);
//        }
//        
//        [TestMethod]
//        public void ImportIndexItemParse_filenamebase_filename_Test()
//        {
//            var createAnImage = new CreateAnImage();
//            var importItem = new ImportIndexItem();
//            AppSettingsProvider.Structure = "/\\t\\z/\\a\\b{filenamebase}/{filenamebase}.ext";
//
//            importItem.SourceFullFilePath = createAnImage.FullFilePath;
//            AppSettingsProvider.BasePath = createAnImage.BasePath;
//            var fileName = importItem.ParseFileName();
//            Assert.AreEqual(createAnImage.DbPath.Replace("/",string.Empty),fileName);
//        }
//        
//        [TestMethod]
//        public void ImportIndexItemParse_filenamebase_subfolder_Test()
//        {
//            var createAnImage = new CreateAnImage();
//            var importItem = new ImportIndexItem();
//            AppSettingsProvider.Structure = "/{filenamebase}/{filenamebase}.ext";
//
//            importItem.SourceFullFilePath = createAnImage.FullFilePath;
//            AppSettingsProvider.BasePath = createAnImage.BasePath;
//            var subfolders = importItem.ParseSubfolders(false);
//            Assert.AreEqual("/" + createAnImage.DbPath.Replace("/",string.Empty).Replace(".jpg",string.Empty) + "/",subfolders);
//        }
//
//        [TestMethod]
//        [ExpectedException(typeof(FileNotFoundException))]
//        public void ImportIndexItemParse_FileNotExist_Test()
//        {
//            AppSettingsProvider.Structure = "/yyyyMMdd_HHmmss.ext";
//
//            var input = new ImportIndexItem
//            {
//                SourceFullFilePath = Path.DirectorySeparatorChar + "20180101_011223.jpg"
//            };
//
//            input.ParseFileName();
//            // ExpectedException
//        }
//
//        [TestMethod]
//        public void ImportIndexItemParse_ParseDateTimeFromFileName_Test()
//        {
//
//            AppSettingsProvider.Structure = "/yyyyMMdd_HHmmss.ext";
//            
//            var input = new ImportIndexItem
//            {
//                SourceFullFilePath = Path.DirectorySeparatorChar + "20180101_011223.jpg"
//            };
//            
//            input.ParseDateTimeFromFileName();
//            
//            DateTime.TryParseExact(
//                "20180101_011223", 
//                "yyyyMMdd_HHmmss",
//                CultureInfo.InvariantCulture, 
//                DateTimeStyles.None, 
//                out var anserDateTime);
//            
//            Assert.AreEqual(anserDateTime,input.DateTime);
//        }
//
//        
//        [TestMethod]
//        public void ImportIndexItemParse_ParseDateTimeFromFileName_WithExtraDotsInName_Test()
//        {
//
//            AppSettingsProvider.Structure = "/yyyyMMdd_HHmmss.ext";
//            
//            var input = new ImportIndexItem
//            {
//                SourceFullFilePath = Path.DirectorySeparatorChar + "2018-02-03 18.47.35.jpg"
//            };
//            
//            input.ParseDateTimeFromFileName();
//            
//            Regex pattern = new Regex("-|_| |;|\\.|:");
//            var output = pattern.Replace("2018-02-03 18.47.35.jpg",string.Empty);
//            
//            DateTime.TryParseExact(
//                "20180203_184735", 
//                "yyyyMMdd_HHmmss",
//                CultureInfo.InvariantCulture, 
//                DateTimeStyles.None, 
//                out var anserDateTime);
//            
//            Assert.AreEqual(anserDateTime,input.DateTime);
//        }
//        
//        
//    }
//}