#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
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
		private readonly Exception? _exception;

		public FakeINotificationQuery(IReadOnlyCollection<NotificationItem>? notificationItem = null)
		{
			if ( notificationItem == null )
			{
				return;
			}
			FakeContent.AddRange(notificationItem);
		}

		public FakeINotificationQuery()
		{
			// nothing here
		}

		public FakeINotificationQuery(Exception? exception = null)
		{
			_exception = exception;
		}

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

		public Task<List<NotificationItem>> GetNewerThan(DateTime parsedDateTime)
		{
			return Task.FromResult(FakeContent.Where(x => x.DateTime > parsedDateTime).ToList());
		}
		
		public Task<List<NotificationItem>> GetOlderThan(DateTime parsedDateTime)
		{
			return Task.FromResult(FakeContent.Where(x => x.DateTime < parsedDateTime).ToList());
		}

		public Task RemoveAsync(IEnumerable<NotificationItem> content)
		{
			if ( _exception != null )
			{
				throw _exception;
			}
			
			foreach ( var contentItem in content )
			{
				if ( FakeContent.Contains(contentItem) )
				{	
					FakeContent.Remove(contentItem);
				}
			}
			
			return Task.CompletedTask;
		}
	}
}

