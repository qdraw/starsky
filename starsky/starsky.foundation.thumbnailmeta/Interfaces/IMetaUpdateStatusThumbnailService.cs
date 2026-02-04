using System.Collections.Generic;
using System.Threading.Tasks;

namespace starsky.foundation.thumbnailmeta.Interfaces;

public interface IMetaUpdateStatusThumbnailService
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="statusResultsWithSubPaths">fail/pass, right type, string=subPath, string?2= error reason</param>
	/// <returns></returns>
	Task UpdateStatusThumbnail(List<(bool, bool, string, string?)> statusResultsWithSubPaths);
}
