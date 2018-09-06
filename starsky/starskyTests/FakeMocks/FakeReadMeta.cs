using System.Collections.Generic;
using starsky.Interfaces;
using starsky.Models;

namespace starskytests.FakeMocks
{
    public class FakeReadMeta : IReadMeta
    {
        public FileIndexItem ReadExifAndXmpFromFile(string singleFilePath)
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
    }
}