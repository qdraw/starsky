using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;

namespace starsky.foundation.database.Notifications
{
	[Service(typeof(INotificationQuery), InjectionLifetime = InjectionLifetime.Scoped)]
	public sealed class NotificationQuery : INotificationQuery
	{
		private readonly ApplicationDbContext _context;
		private readonly IWebLogger _logger;

		public NotificationQuery(ApplicationDbContext context, IWebLogger logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task<NotificationItem> AddNotification(string content)
		{
			var item = new NotificationItem
			{
				DateTime = DateTime.UtcNow,
				DateTimeEpoch = DateTimeOffset.Now.ToUnixTimeSeconds(),
				Content = content
			};
			
			try
			{
				await _context.Notifications.AddAsync(item);
				await _context.SaveChangesAsync();
			}
			catch ( DbUpdateConcurrencyException concurrencyException )
			{
				_logger.LogInformation("[AddNotification] try to fix DbUpdateConcurrencyException", concurrencyException);
				SolveConcurrency.SolveConcurrencyExceptionLoop(concurrencyException.Entries);
				try
				{
					await _context.SaveChangesAsync();
				}
				catch ( DbUpdateConcurrencyException e)
				{
					_logger.LogInformation(e, "[AddNotification] save failed after DbUpdateConcurrencyException");
				}
			}
			
			return item;
		}

		public Task<NotificationItem> AddNotification<T>(ApiNotificationResponseModel<T> content)
		{
			var stringMessage = JsonSerializer.Serialize(content,
				DefaultJsonSerializer.CamelCase);		
			return AddNotification(stringMessage);
		}

		public Task<List<NotificationItem>> GetNewerThan(DateTime parsedDateTime)
		{
			var unixTime = ((DateTimeOffset)parsedDateTime).ToUnixTimeSeconds() -1;
			return _context.Notifications.Where(x => x.DateTimeEpoch > unixTime).ToListAsync();
		}

		public Task<List<NotificationItem>> GetOlderThan(DateTime parsedDateTime)
		{
			var unixTime = ((DateTimeOffset)parsedDateTime).ToUnixTimeSeconds();
			return _context.Notifications.Where(x => x.DateTimeEpoch < unixTime).ToListAsync();
		}

		public async Task RemoveAsync(IEnumerable<NotificationItem> content)
		{
			_context.Notifications.RemoveRange(content);
			await _context.SaveChangesAsync();
		}
	}
}

