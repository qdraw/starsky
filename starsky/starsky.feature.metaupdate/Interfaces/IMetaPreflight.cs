using System.Collections.Generic;
using starsky.foundation.database.Models;

namespace starsky.feature.metaupdate.Interfaces
{
	public interface IMetaPreflight
	{
		(List<FileIndexItem> fileIndexResultsList, Dictionary<string, List<string>> changedFileIndexItemName)
			Preflight(FileIndexItem inputModel, string[] inputFilePaths, bool append,
				bool collections, int rotateClock);
		void CompareAllLabelsAndRotation(Dictionary<string, List<string>> changedFileIndexItemName, 
			DetailView detailView, FileIndexItem inputModel, bool append, int rotateClock);
	}
}
