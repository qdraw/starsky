using System.Collections.Generic;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskytests.FakeCreateAn;

namespace starskytests.FakeMocks
{
    public class FakeReadMeta : IReadMeta
    {
        public FileIndexItem ReadExifAndXmpFromFile(string singleFilePath, Files.ImageFormat imageFormat)
        {
            return new FileIndexItem{Status = FileIndexItem.ExifStatus.Ok, Tags = "test", FileHash = "test", FileName = "t", ParentDirectory = "d"};
        }


        public List<FileIndexItem> ReadExifAndXmpFromFileAddFilePathHash(string[] fullFilePathArray)
        {
            var createAnImage = new CreateAnImage();
            return new List<FileIndexItem> {new FileIndexItem{Status = FileIndexItem.ExifStatus.Ok, FileName = createAnImage.FileName}};
        }

        public void RemoveReadMetaCache(List<string> fullFilePathArray)
        {
            // dont do anything
        }

        public void RemoveReadMetaCache(string fullFilePath)
        {
            // dont do anything
        }

        public void UpdateReadMetaCache(string fullFilePath, FileIndexItem objectExifToolModel)
        {
            // dont do anything
        }
    }
}
