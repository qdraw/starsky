using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;

namespace starsky.foundation.database.Interfaces
{
	public interface INotificationQuery
	{
		Task<NotificationItem> AddNotification(string content);
		Task<NotificationItem> AddNotification<T>(ApiNotificationResponseModel<T> content);
	}
}

