using starsky.foundation.database.Models;

namespace starsky.foundation.metaupdate.Interfaces;

public interface IMetaReplaceService
{
	Task<List<FileIndexItem>> Replace(string f, string fieldName,
		string search, string? replace, bool collections);
}
