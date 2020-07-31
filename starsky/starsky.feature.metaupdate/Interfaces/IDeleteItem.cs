using System.Collections.Generic;
using starsky.foundation.database.Models;

namespace starsky.feature.metaupdate.Interfaces
{
	public interface IDeleteItem
	{
		List<FileIndexItem> Delete(string f, bool collections);
	}
}
