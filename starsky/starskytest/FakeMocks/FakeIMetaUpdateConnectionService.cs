using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Interfaces;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Models;

namespace starskytest.FakeMocks;

public class FakeIMetaUpdateConnectionService : IMetaUpdateConnectionService
{
	public List<List<FileIndexItem>> Sent { get; } = [];

	public Task<ApiNotificationResponseModel<List<FileIndexItem>>> UpdateWebSocketTaskRun(
		List<FileIndexItem> fileIndexResultsList)
	{
		Sent.Add(fileIndexResultsList);
		return Task.FromResult(new ApiNotificationResponseModel<List<FileIndexItem>>(
			fileIndexResultsList, ApiNotificationType.MetaUpdate));
	}
}
