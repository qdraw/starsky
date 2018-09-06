using System.Collections.Generic;
using starsky.Interfaces;
using starsky.Models;

namespace starskytests.FakeMocks
{
    public class FakeReadMeta : IReadMeta
    {
        public FileIndexItem ReadExifAndXmpFromFile(string singleFilePath)
        {
            return new FileIndexItem{Tags = "test"};
        }

        public List<FileIndexItem> ReadExifAndXmpFromFileAddFilePathHash(string[] fullFilePathArray)
        {
            return new List<FileIndexItem>();
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