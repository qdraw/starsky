using System.Collections.Generic;
using starskycore.Helpers;
using starskycore.Models;

namespace starskycore.Interfaces
{
    public interface IReadMeta
    {
        // this returns only meta data > so no filename or filehash
        FileIndexItem ReadExifAndXmpFromFile(string fullFilePath, ExtensionRolesHelper.ImageFormat imageFormat);
        List<FileIndexItem> ReadExifAndXmpFromFileAddFilePathHash(List<string> subPathArray, List<string> fileHashes = null);
        void RemoveReadMetaCache(string fullFilePath);
        void UpdateReadMetaCache(string fullFilePath, FileIndexItem objectExifToolModel);
    }
}