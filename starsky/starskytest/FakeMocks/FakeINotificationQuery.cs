using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;

namespace starskytest.FakeMocks
{
	public class FakeINotificationQuery : INotificationQuery
	{
		public List<NotificationItem> FakeContent { get; set; } = new List<NotificationItem>();

		public Task<NotificationItem> AddNotification(string content)
		{
			var item = new NotificationItem
			{
				DateTime = DateTime.UtcNow,
				Content = content
			};
			FakeContent.Add(item);
			return Task.FromResult(item);
		}

		public Task<NotificationItem> AddNotification<T>(ApiNotificationResponseModel<T> content)
		{
			var stringMessage = JsonSerializer.Serialize(content,
				DefaultJsonSerializer.CamelCase);		
			return AddNotification(stringMessage);
		}

		public Task<List<NotificationItem>> Get(DateTime parsedDateTime)
		{
			throw new NotImplementedException();
		}
	}
}

