using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.foundation.database.Interfaces
{
	public interface INotificationQuery
	{
		Task<NotificationItem> AddNotification(string content);
	}
}

