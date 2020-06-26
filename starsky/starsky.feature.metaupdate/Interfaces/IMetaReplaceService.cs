using System.Collections.Generic;
using starsky.foundation.database.Models;

namespace starsky.feature.update.Interfaces
{
	public interface IMetaReplaceService
	{
		List<FileIndexItem> Replace(string f, string fieldName, string search, string replace,
			bool collections);
	}
}
