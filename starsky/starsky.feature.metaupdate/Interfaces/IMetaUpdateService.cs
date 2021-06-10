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
		/// <param name="fileIndexResultsList">object that are changed</param>
		/// <param name="inputModel"></param>
		/// <param name="collections"></param>
		/// <param name="append"></param>
		/// <param name="rotateClock"></param>
		/// <returns></returns>
		Task<List<FileIndexItem>> Update(
			List<FileIndexItem> fileIndexResultsList,
			FileIndexItem inputModel,
			bool collections, bool append, int rotateClock);

		void UpdateReadMetaCache(IEnumerable<FileIndexItem> returnNewResultList);
	}
}
