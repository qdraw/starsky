using System.Collections.Generic;
using starsky.foundation.database.Models;

namespace starsky.feature.metaupdate.Interfaces
{
	public interface IMetaUpdateService
	{
		List<DetailView> Update(Dictionary<string, List<string>> changedFileIndexItemName,
			List<FileIndexItem> fileIndexResultsList,
			FileIndexItem inputModel,
			bool collections, bool append, int rotateClock);
	}
}
