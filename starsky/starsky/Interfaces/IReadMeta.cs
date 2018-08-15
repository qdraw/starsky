using System.Collections.Generic;
using starsky.Models;

namespace starsky.Interfaces
{
    public interface IReadMeta
    {
        FileIndexItem ReadExifAndXmpFromFile(string singleFilePath);
        void RemoveReadMetaCache(List<string> fullFilePathList);
    }
}