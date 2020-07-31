using System.Collections.Generic;
using starsky.foundation.database.Models;

namespace starsky.feature.metaupdate.Interfaces
{
	public interface IMetaInfo
	{
		List<FileIndexItem> GetInfo(List<string> inputFilePaths, bool collections);
	}
}
