using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using starsky.feature.realtime.Interface;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;

namespace starskytest.FakeMocks
{
	public class FakeIRealtimeConnectionsService : IRealtimeConnectionsService
	{
		public List<string> FakeSendToAllAsync { get; set; } = new List<string>();

		public Task NotificationToAllAsync<T>(ApiNotificationResponseModel<T> message,
			CancellationToken cancellationToken)
		{
			FakeSendToAllAsync.Add(JsonSerializer.Serialize(
				message, DefaultJsonSerializer.CamelCase));
			return Task.CompletedTask;
		}

		public Task CleanOldMessagesAsync()
		{
			throw new System.NotImplementedException();
		}
	}
}

