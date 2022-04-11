using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.platform.Models;

namespace starsky.feature.realtime.Interface
{
	public interface IRealtimeConnectionsService
	{
		Task NotificationToAllAsync<T>(ApiNotificationResponseModel<T> message, CancellationToken cancellationToken);
	}
}

