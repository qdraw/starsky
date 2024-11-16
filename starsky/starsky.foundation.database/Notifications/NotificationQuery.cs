using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;

namespace starsky.foundation.database.Notifications;

[Service(typeof(INotificationQuery), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class NotificationQuery : INotificationQuery
{
	private readonly ApplicationDbContext _context;
	private readonly IWebLogger _logger;
	private readonly IServiceScopeFactory _scopeFactory;

	public NotificationQuery(ApplicationDbContext context, IWebLogger logger,
		IServiceScopeFactory scopeFactory)
	{
		_context = context;
		_logger = logger;
		_scopeFactory = scopeFactory;
	}

	public Task<NotificationItem> AddNotification<T>(ApiNotificationResponseModel<T> content)
	{
		var stringMessage = JsonSerializer.Serialize(content,
			DefaultJsonSerializer.CamelCaseNoEnters);
		return AddNotification(stringMessage);
	}

	public Task<List<NotificationItem>> GetNewerThan(DateTime parsedDateTime)
	{
		var unixTime = ( ( DateTimeOffset ) parsedDateTime ).ToUnixTimeSeconds() - 1;
		return _context.Notifications.Where(x => x.DateTimeEpoch > unixTime).ToListAsync();
	}

	public Task<List<NotificationItem>> GetOlderThan(DateTime parsedDateTime)
	{
		var unixTime = ( ( DateTimeOffset ) parsedDateTime ).ToUnixTimeSeconds();
		return _context.Notifications.Where(x => x.DateTimeEpoch < unixTime).ToListAsync();
	}

	public async Task RemoveAsync(IEnumerable<NotificationItem> content)
	{
		_context.Notifications.RemoveRange(content);
		await _context.SaveChangesAsync();
	}

	public async Task<NotificationItem> AddNotification(string content)
	{
		var item = new NotificationItem
		{
			DateTime = DateTime.UtcNow,
			DateTimeEpoch = DateTimeOffset.Now.ToUnixTimeSeconds(),
			Content = content
		};

		return await RetryHelper.DoAsync(LocalAddQuery, TimeSpan.FromSeconds(1));

		async Task<NotificationItem> LocalAdd(ApplicationDbContext context)
		{
			try
			{
				context.Entry(item).State = EntityState.Added;
				await context.Notifications.AddAsync(item);
				await context.SaveChangesAsync();
				return item;
			}
			catch ( DbUpdateException updateException )
			{
				if ( updateException is DbUpdateConcurrencyException )
				{
					_logger.LogInformation(
						"[AddNotification] try to fix DbUpdateConcurrencyException",
						updateException);
					SolveConcurrency.SolveConcurrencyExceptionLoop(updateException.Entries);
					try
					{
						await _context.SaveChangesAsync();
					}
					catch ( DbUpdateConcurrencyException e )
					{
						_logger.LogInformation(e,
							"[AddNotification] save failed after DbUpdateConcurrencyException");
					}
				}
				else if ( updateException.InnerException is SqliteException
				         {
					         SqliteErrorCode: 19
				         } )
				{
					_logger.LogInformation($"[AddNotification] SqliteException retry next: " +
					                       $"{updateException.InnerException.Message}");

					item.Id = 0;
					await LocalAddQuery();
				}
				else
				{
					_logger.LogInformation(updateException,
						"[AddNotification] save failed");
					throw;
				}
			}

			return item;
		}

		async Task<NotificationItem> LocalAddQuery()
		{
			try
			{
				return await LocalAdd(_context);
			}
			catch ( ObjectDisposedException )
			{
				// Include create new scope factory
				var context = new InjectServiceScope(_scopeFactory).Context();
				return await LocalAdd(context);
			}
		}
	}
}
