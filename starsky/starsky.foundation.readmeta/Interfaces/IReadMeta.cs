﻿using System.Collections.Generic;
using starsky.foundation.database.Models;

namespace starsky.foundation.readmeta.Interfaces
{
    public interface IReadMeta
    {
		/// <summary>
		/// this returns only meta data > so no fileName or fileHash
		/// </summary>
		/// <param name="subPath">subPath</param>
		/// <returns></returns>
	    FileIndexItem ReadExifAndXmpFromFile(string subPath);
        List<FileIndexItem> ReadExifAndXmpFromFileAddFilePathHash(List<string> subPathList, List<string> fileHashes = null);
        void RemoveReadMetaCache(string fullFilePath);
        void UpdateReadMetaCache(string fullFilePath, FileIndexItem objectExifToolModel);
    }
}
