using System;
using System.Threading.Tasks;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;

namespace starsky.foundation.database.Notifications
{
	public class NotificationQuery : INotificationQuery
	{
		private readonly ApplicationDbContext _context;
		private readonly AppSettings _appSettings;

		public NotificationQuery(ApplicationDbContext context, 
			AppSettings appSettings)
		{
			_context = context;
			_appSettings = appSettings;
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
	}
	


}

