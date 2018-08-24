using System.Collections.Generic;
using starsky.Models;

namespace starsky.Interfaces
{
    public interface IReadMeta
    {
        // this returns only meta data > so no filename or filehash
        FileIndexItem ReadExifAndXmpFromFile(string fullFilePath);
        void RemoveReadMetaCache(List<string> fullFilePathList);
    }
}