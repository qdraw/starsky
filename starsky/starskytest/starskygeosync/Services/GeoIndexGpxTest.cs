using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Helpers;
using starskycore.Models;
using starskycore.Services;
using starskyGeoCli.Services;
using starskytests.FakeCreateAn;


namespace starskytests.starskygeosync.Services
{
    [TestClass]
    public class GeoIndexGpxTest
    {
        private readonly AppSettings _appSettings;
        private readonly ReadMeta _readMeta;
        private readonly List<FileIndexItem> _metaFilesDirectory;

        public GeoIndexGpxTest()
        {
            var createAnGpx = new CreateAnGpx();
            _appSettings = new AppSettings
            {
                StorageFolder = createAnGpx.BasePath,
                CameraTimeZone = "Europe/Minsk"
            };
            _readMeta = new ReadMeta(_appSettings);

            _metaFilesDirectory = new List<FileIndexItem>
            {
                new FileIndexItem
                {
                    FileName = createAnGpx.FileName,
                    ImageFormat = FilesHelper.ImageFormat.gpx
                }
            };

        }
        [TestMethod]
        public void GeoIndexGpx_LoopFolderLookupTest()
        {
            var exampleFiles = new List<FileIndexItem>(); 
            exampleFiles.AddRange(new List<FileIndexItem>
            {
                _metaFilesDirectory.FirstOrDefault(),
                new FileIndexItem
                {
                    FileName = "01.jpg", 
                    DateTime = new DateTime(2018,09,05,20,31,54) // 2018-09-05T17:31:53Z UTC > In europe/Minsk
                },
                new FileIndexItem
                {
                    FileName = "NotInRange.jpg", 
                    DateTime = new DateTime(2018,09,06,00,00,00)
                }
                
            });
               
            var returnFileIndexItems = new GeoIndexGpx(_appSettings,_readMeta).LoopFolder(exampleFiles);
            
            Assert.AreEqual(null,returnFileIndexItems.FirstOrDefault(p => p.FileName == "NotInRange.jpg"));
            Assert.AreEqual("01.jpg",returnFileIndexItems.FirstOrDefault(p => p.FileName == "01.jpg")?.FileName);


        }

    }
}
