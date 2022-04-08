using System;
using System.Text.Json;
using System.Threading.Tasks;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;

namespace starsky.foundation.database.Notifications
{
	[Service(typeof(INotificationQuery), InjectionLifetime = InjectionLifetime.Scoped)]
	public class NotificationQuery : INotificationQuery
	{
		private readonly ApplicationDbContext _context;

		public NotificationQuery(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<NotificationItem> AddNotification(string content)
		{
			var item = new NotificationItem
			{
				DateTime = DateTime.UtcNow,
				Content = content
			};
			await _context.Notifications.AddAsync(item);
			return item;
		}

		public Task<NotificationItem> AddNotification(ApiNotificationResponseModel content)
		{
			var stringMessage = JsonSerializer.Serialize(content,
				DefaultJsonSerializer.CamelCase);		
			return AddNotification(stringMessage);
		}
	}
}

