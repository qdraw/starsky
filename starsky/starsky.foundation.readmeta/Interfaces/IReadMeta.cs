#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
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
	    Task<FileIndexItem?> ReadExifAndXmpFromFileAsync(string subPath);
	    Task<List<FileIndexItem>> ReadExifAndXmpFromFileAddFilePathHashAsync(List<string> subPathList, List<string>? fileHashes = null);
        bool? RemoveReadMetaCache(string fullFilePath);
        void UpdateReadMetaCache(IEnumerable<FileIndexItem> objectExifToolModel);
    }
}
