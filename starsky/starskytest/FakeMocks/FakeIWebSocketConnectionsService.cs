using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Helpers;
using starsky.foundation.realtime.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeIWebSocketConnectionsService : IWebSocketConnectionsService
	{
		public void AddConnection(WebSocketConnection connection)
		{
			throw new NotImplementedException();
		}

		public void RemoveConnection(Guid connectionId)
		{
			throw new NotImplementedException();
		}

		public List<string> FakeSendToAllAsync { get; set; } = new List<string>();
		
		public Task SendToAllAsync(string message, CancellationToken cancellationToken)
		{
			FakeSendToAllAsync.Add(message);
			return Task.CompletedTask;
		}

		public Task SendToAllAsync<T>(ApiNotificationResponseModel<T> message, CancellationToken cancellationToken)
		{
			var stringMessage = JsonSerializer.Serialize(message,
				DefaultJsonSerializer.CamelCase);
			FakeSendToAllAsync.Add(stringMessage);
			return Task.CompletedTask;
		}
	}
}
