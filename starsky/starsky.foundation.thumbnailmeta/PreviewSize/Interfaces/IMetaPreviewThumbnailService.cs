using System.Collections.Generic;
using System.Threading.Tasks;

namespace starsky.foundation.thumbnailmeta.ServicesPreviewSize.Interfaces;

public interface IMetaPreviewThumbnailService
{
	/// <summary>
	///     File
	/// </summary>
	/// <param name="subPathsAndHash">(FilePath,FileHash)</param>
	/// <returns>fail/pass, right type, string=subPath, string?2= error reason</returns>
	Task<IEnumerable<(bool, bool, string, string?)>> AddPreviewThumbnail(
		IEnumerable<(string, string)> subPathsAndHash);

	/// <summary>
	///     add meta thumbnail to a single file
	/// </summary>
	/// <param name="subPath">subPath</param>
	/// <returns>fail/pass, right type, string=subPath, string?2= error reason</returns>
	Task<List<(bool, bool, string, string?)>> AddPreviewThumbnail(string subPath);

	/// <summary>
	///     Add Meta Thumbnail to a single file by fileHash
	/// </summary>
	/// <param name="subPath">subPath</param>
	/// <param name="fileHash">hash</param>
	/// <returns>fail/pass, right type, string=subPath, string?2= error reason</returns>
	Task<(bool, bool, string, string?)> AddPreviewThumbnail(string subPath, string fileHash);
}
