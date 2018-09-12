using System.Collections.Generic;
using starsky.Helpers;
using starsky.Models;

namespace starsky.Interfaces
{
    public interface IReadMeta
    {
        // this returns only meta data > so no filename or filehash
        FileIndexItem ReadExifAndXmpFromFile(string fullFilePath, Files.ImageFormat imageFormat);
        List<FileIndexItem> ReadExifAndXmpFromFileAddFilePathHash(string[] fullFilePathArray);
        void RemoveReadMetaCache(string fullFilePath);
        void UpdateReadMetaCache(string fullFilePath, FileIndexItem objectExifToolModel);
    }
}