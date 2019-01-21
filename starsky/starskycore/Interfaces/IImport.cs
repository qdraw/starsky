using System;
using System.Collections.Generic;
using starskycore.Models;

namespace starskycore.Interfaces
{
    public interface IImport
    {
        List<string> Import(IEnumerable<string> inputFullPathList, ImportSettingsModel importSettings);
        ImportIndexItem GetItemByHash(string fileHash);
        FileIndexItem ReadExifAndXmpFromFile(string inputFileFullPath);
        ImportIndexItem ObjectCreateIndexItem(
            string inputFileFullPath,
            string fileHashCode,
            FileIndexItem fileIndexItem,
            string overwriteStructure);
        bool IsAgeFileFilter(ImportSettingsModel importSettings, DateTime exifDateTime);
    }
}