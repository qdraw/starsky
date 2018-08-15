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
    }
}