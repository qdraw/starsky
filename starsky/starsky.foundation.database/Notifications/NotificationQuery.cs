using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
			await _context.SaveChangesAsync();
			
			return item;
		}

		public Task<NotificationItem> AddNotification<T>(ApiNotificationResponseModel<T> content)
		{
			var stringMessage = JsonSerializer.Serialize(content,
				DefaultJsonSerializer.CamelCase);		
			return AddNotification(stringMessage);
		}

		public Task<List<NotificationItem>> Get(DateTime parsedDateTime)
		{
			return _context.Notifications.Where(x => x.DateTime > parsedDateTime).ToListAsync();
		}
	}
}

