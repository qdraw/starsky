using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
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
	internal const string ErrorMessageContentToLong = "Serialized content is too large";
	private readonly ApplicationDbContext _context;
	private readonly IWebLogger _logger;
	private readonly IServiceScopeFactory _scopeFactory;

	/// <summary>
	/// should be lower than MEDIUMTEXT: 5_000_000 is 5MB
	/// </summary>
	private const int MaxContentLength = 5_000_000;

	public NotificationQuery(ApplicationDbContext context, IWebLogger logger,
		IServiceScopeFactory scopeFactory)
	{
		_context = context;
		_logger = logger;
		_scopeFactory = scopeFactory;
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

	/// <summary>
	///     Add notification to the database
	/// </summary>
	/// <param name="content">Content</param>
	/// <typeparam name="T">Type</typeparam>
	/// <returns>DatabaseItem</returns>
	public Task<NotificationItem> AddNotification<T>(ApiNotificationResponseModel<T> content)
	{
		var stringMessage = JsonSerializer.Serialize(content,
			DefaultJsonSerializer.CamelCaseNoEnters);
		return AddNotification(stringMessage);
	}

	/// <summary>
	///     Add content to database
	/// </summary>
	/// <param name="content">json content</param>
	/// <returns>item with id</returns>
	public async Task<NotificationItem> AddNotification(string content)
	{
		var item = NewNotificationItem(content);

		if ( content.Length <= MaxContentLength )
		{
			return await RetryHelper.DoAsync(LocalAddQuery, TimeSpan.FromSeconds(1));
		}

		_logger.LogError($"[NotificationQuery]: {ErrorMessageContentToLong} " +
		                 $"{content.Length} - First 3000 chars: {content[..3000]}" +
		                 $" so skipping add notification");
		return item;

		async Task<NotificationItem> LocalAddQuery()
		{
			return await AddNotification(_context, item, content);
		}
	}

	internal static NotificationItem NewNotificationItem(string content)
	{
		return new NotificationItem
		{
			DateTime = DateTime.UtcNow,
			DateTimeEpoch = DateTimeOffset.Now.ToUnixTimeSeconds(),
			Content = content.Length < MaxContentLength ? content : ""
		};
	}

	internal async Task<NotificationItem> AddNotification(ApplicationDbContext context,
		NotificationItem item, string content)
	{
		try
		{
			return await LocalAddQuery(item);
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
			else if ( updateException.InnerException is MySqlException
			         {
				         ErrorCode: MySqlErrorCode.DuplicateKeyEntry
			         } )
			{
				_logger.LogInformation($"[AddNotification] MySqlException retry next: " +
				                       $"{updateException.InnerException.Message}");
				return await LocalAddQuery(NewNotificationItem(content));
			}
			else if ( updateException.InnerException is SqliteException
			         {
				         SqliteErrorCode: 19
			         } )
			{
				_logger.LogInformation($"[AddNotification] SqliteException retry next: " +
				                       $"{updateException.InnerException.Message}");

				item.Id = 0;
				return await LocalAddQuery(item);
			}
			else
			{
				_logger.LogError(updateException,
					$"[AddNotification] no solution maybe retry? " +
					$"M: {updateException.Message}");
				throw;
			}
		}

		return item;

		async Task<NotificationItem> LocalAddQuery(NotificationItem addItem)
		{
			try
			{
				context.Entry(addItem).State = EntityState.Added;
				await context.Notifications.AddAsync(addItem);
				await context.SaveChangesAsync();
				return addItem;
			}
			catch ( ObjectDisposedException )
			{
				// Include create new scope factory
				var dbContext = new InjectServiceScope(_scopeFactory).Context();
				dbContext.Entry(addItem).State = EntityState.Added;
				await dbContext.Notifications.AddAsync(addItem);
				await dbContext.SaveChangesAsync();
				return addItem;
			}
		}
	}
}
