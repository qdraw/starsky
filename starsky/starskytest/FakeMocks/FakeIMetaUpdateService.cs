using System.Collections.Generic;
using starsky.feature.metaupdate.Interfaces;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks
{
	public class FakeIMetaUpdateService : IMetaUpdateService
	{
		public List<FileIndexItem> Update(Dictionary<string, List<string>> changedFileIndexItemName, List<FileIndexItem> fileIndexResultsList,
			FileIndexItem inputModel, bool collections, bool append, int rotateClock)
		{
			// does not update yet
			return fileIndexResultsList;
		}

		public void UpdateReadMetaCache(IEnumerable<FileIndexItem> returnNewResultList)
		{
			throw new System.NotImplementedException();
		}
	}
}
