using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;

namespace starsky.foundation.metaupdate.Interfaces;

public interface IMetaUpdateConnectionService
{
	Task<ApiNotificationResponseModel<List<FileIndexItem>>> UpdateWebSocketTaskRun(
		List<FileIndexItem> fileIndexResultsList);
}
