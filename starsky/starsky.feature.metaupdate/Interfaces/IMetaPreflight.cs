using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.feature.metaupdate.Interfaces
{
	public interface IMetaPreflight
	{
		Task<(List<FileIndexItem> fileIndexResultsList, Dictionary<string, List<string>> changedFileIndexItemName)>
			Preflight(FileIndexItem inputModel, string[] inputFilePaths,
				bool append,
				bool collections, int rotateClock);

		void CompareAllLabelsAndRotation(Dictionary<string, List<string>> changedFileIndexItemName,
			FileIndexItem collectionsFileIndexItem, FileIndexItem statusModel, bool append,
			int rotateClock);
	}
}
