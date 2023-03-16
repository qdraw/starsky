using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;

namespace starsky.foundation.database.Interfaces
{
	public interface INotificationQuery
	{
		Task<NotificationItem> AddNotification<T>(ApiNotificationResponseModel<T> content);
		Task<List<NotificationItem>> GetNewerThan(DateTime parsedDateTime);
		Task<List<NotificationItem>> GetOlderThan(DateTime parsedDateTime);
		Task RemoveAsync(IEnumerable<NotificationItem> content);

	}
}

