using System;
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
		public List<Tuple<string,DateTime>> FakeSendToAllAsync { get; set; } = new List<Tuple<string,DateTime>>();

		public Task NotificationToAllAsync<T>(ApiNotificationResponseModel<T> message,
			CancellationToken cancellationToken)
		{
			FakeSendToAllAsync.Add(new Tuple<string, DateTime>(JsonSerializer.Serialize(message), DateTime.UtcNow));
			return Task.CompletedTask;
		}

		public Task CleanOldMessagesAsync()
		{
			FakeSendToAllAsync.RemoveAll(p => p.Item2 < DateTime.UtcNow.AddDays(-30));
			return Task.CompletedTask;
		}
	}
}

