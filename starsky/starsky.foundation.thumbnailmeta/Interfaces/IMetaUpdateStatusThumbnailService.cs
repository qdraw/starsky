using System.Collections.Generic;
using System.Threading.Tasks;

namespace starsky.foundation.metathumbnail.Interfaces;

public interface IMetaUpdateStatusThumbnailService
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="statusResultsWithSubPaths">fail/pass, string=subPath, string?2= error reason</param>
	/// <returns></returns>
	Task UpdateStatusThumbnail(List<(bool, string, string?)> statusResultsWithSubPaths);
}
