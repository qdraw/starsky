using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.feature.metaupdate.Interfaces
{
	public interface IMetaUpdateService
	{
		/// <summary>
		/// To Actual update the metadata
		/// </summary>
		/// <param name="changedFileIndexItemName"></param>
		/// <param name="fileIndexResultsList">object that are changed</param>
		/// <param name="inputModel"></param>
		/// <param name="collections"></param>
		/// <param name="append"></param>
		/// <param name="rotateClock"></param>
		/// <returns></returns>
		Task<List<FileIndexItem>> Update(
			Dictionary<string,List<string>> changedFileIndexItemName,
			List<FileIndexItem> fileIndexResultsList,
			FileIndexItem inputModel, // only when changedFileIndexItemName = null
			bool collections, bool append, // only when changedFileIndexItemName = null
			int rotateClock);

		void UpdateReadMetaCache(IEnumerable<FileIndexItem> returnNewResultList);
	}
}
