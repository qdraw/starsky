using System.Collections.Generic;
using System.Threading.Tasks;

namespace starsky.foundation.metathumbnail.Interfaces
{
	public interface IMetaExifThumbnailService
	{
		/// <summary>
		/// File
		/// </summary>
		/// <param name="subPathsAndHash">(FilePath,FileHash)</param>
		/// <returns>fail/pass, string=subPath, string?2= error reason</returns>
		Task<IEnumerable<(bool, string, string?)>> AddMetaThumbnail(IEnumerable<(string, string)> subPathsAndHash);
		
		/// <summary>
		/// add meta thumbnail to a single file
		/// </summary>
		/// <param name="subPath">subPath</param>
		/// <returns>fail/pass, string=subPath, string?2= error reason</returns>
		Task<List<(bool, string, string?)>> AddMetaThumbnail(string subPath);
		
		/// <summary>
		/// Add Meta Thumbnail to a single file by fileHash
		/// </summary>
		/// <param name="subPath">subPath</param>
		/// <param name="fileHash">hash</param>
		/// <returns>fail/pass, string=subPath, string?2= error reason</returns>
		Task<(bool,string, string?)> AddMetaThumbnail(string subPath, string fileHash);
	}
}
