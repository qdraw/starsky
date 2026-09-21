using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Notifications;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.NotificationsTest;

/// <summary>
///     NotificationQuery
/// </summary>
[TestClass]
public sealed class NotificationQueryTest
{
	private readonly ApplicationDbContext _dbContext;
	private readonly FakeIWebLogger _logger;
	private readonly NotificationQuery _notificationQuery;

	public NotificationQueryTest()
	{
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		_dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var serviceScopeFactory = scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
		_logger = new FakeIWebLogger();
		_notificationQuery =
			new NotificationQuery(_dbContext, new FakeIWebLogger(), serviceScopeFactory);
	}

	public TestContext TestContext { get; set; }

	private static IServiceScopeFactory CreateNewScope()
	{
		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase(nameof(NotificationQueryTest)));
		var serviceProvider = services.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	[TestMethod]
	public async Task ShouldContainWhenAdd()
	{
		await _notificationQuery.AddNotification(
			new ApiNotificationResponseModel<string>("test")
			{
				Type = ApiNotificationType.Welcome
			});

		var testNotification =
			await _dbContext.Notifications.FirstOrDefaultAsync(p =>
				p.Content!.Contains("test"), TestContext.CancellationTokenSource.Token);

		Assert.IsNotNull(testNotification);

		_dbContext.Notifications.Remove(testNotification);
		await _dbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);
	}

	[TestMethod]
	public async Task Disposed()
	{
		var serviceScopeFactory = CreateNewScope();
		var scope = serviceScopeFactory.CreateScope();
		var dbContextDisposed = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		// Dispose here
		await dbContextDisposed.DisposeAsync();

		await new NotificationQuery(dbContextDisposed, _logger, serviceScopeFactory)
			.AddNotification(
				new ApiNotificationResponseModel<string>("test_disposed_notification")
				{
					Type = ApiNotificationType.Welcome
				});

		var injectServiceScope = new InjectServiceScope(serviceScopeFactory);
		var testNotification = await injectServiceScope.ExecuteAsync(async context =>
		{
			var notification = await context.Notifications.FirstOrDefaultAsync(p =>
						p.Content!.Contains("test_disposed_notification"),
					TestContext.CancellationTokenSource.Token);

			// and remove it afterward
			foreach ( var notificationsItem in await context.Notifications.ToListAsync(TestContext
				         .CancellationTokenSource.Token) )
			{
				context.Notifications.Remove(notificationsItem);
			}

			await context.SaveChangesAsync(TestContext.CancellationTokenSource.Token);
			return notification;
		});

		Assert.IsNotNull(testNotification?.Content);
		Assert.IsGreaterThanOrEqualTo(1746613149, testNotification.DateTimeEpoch);
	}

	[TestMethod]
	public async Task AddNotification_Internal_DisposedTest()
	{
		// Arrange
		var serviceScopeFactory = CreateNewScope();
		var scope = serviceScopeFactory.CreateScope();
		var dbContextDisposed = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		const string content = "Test notification disposed";

		// Dispose here
		await dbContextDisposed.DisposeAsync();

		// Act
		var result = new NotificationItem();

		try
		{
			var sut = new NotificationQuery(dbContextDisposed, new FakeIWebLogger(),
				serviceScopeFactory);
			result = await sut.AddNotification(
				NotificationQuery.NewNotificationItem(content), content);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(content, result.Content);
		}
		finally
		{
			var injectServiceScope = new InjectServiceScope(serviceScopeFactory);
			await injectServiceScope.ExecuteAsync(async dbContext =>
			{
				dbContext.Notifications.Remove(result);
				await dbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);
				return true;
			});

			scope.Dispose();
		}
	}

	[TestMethod]
	public async Task AddNotification_Public_DisposedTest2()
	{
		// Arrange
		var serviceScopeFactory = CreateNewScope();
		var scope = serviceScopeFactory.CreateScope();
		var dbContextDisposed = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		const string content = "Test notification disposed";

		// Dispose here
		await dbContextDisposed.DisposeAsync();

		// Act
		var result = new NotificationItem();

		try
		{
			var sut = new NotificationQuery(dbContextDisposed, new FakeIWebLogger(),
				serviceScopeFactory);
			result = await sut.AddNotification(content);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(content, result.Content);
		}
		finally
		{
			var injectServiceScope = new InjectServiceScope(serviceScopeFactory);
			await injectServiceScope.ExecuteAsync(async dbContext =>
			{
				dbContext.Notifications.Remove(result);
				await dbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);
				return true;
			});

			scope.Dispose();
		}
	}

	[TestMethod]
	public async Task Get_RecentItems_Ok()
	{
		var currentTime = DateTime.UtcNow;
		var item = await _notificationQuery.AddNotification(
			new ApiNotificationResponseModel<string>("test")
			{
				Type = ApiNotificationType.Welcome
			});

		var unixTime = ( ( DateTimeOffset ) currentTime ).ToUnixTimeSeconds() - 1;

		Assert.IsLessThanOrEqualTo(item.DateTimeEpoch, unixTime);

		var recent = await _notificationQuery.GetNewerThan(currentTime);
		Assert.HasCount(1, recent);

		_dbContext.Notifications.RemoveRange(recent);
		await _dbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);
	}

	[TestMethod]
	public async Task GetNewerThan_RecentItems_OldIgnored()
	{
		var currentTime = DateTime.UtcNow;
		_dbContext.Notifications.Add(
			new NotificationItem { DateTime = DateTime.UnixEpoch });
		await _dbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		var recent = await _notificationQuery.GetNewerThan(currentTime);
		Assert.IsEmpty(recent);

		_dbContext.Notifications.RemoveRange(_dbContext.Notifications);
		await _dbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);
	}

	[TestMethod]
	public async Task Get_OlderItems()
	{
		var currentTime = DateTime.UtcNow;
		_dbContext.Notifications.Add(
			new NotificationItem { DateTime = DateTime.UtcNow.AddMinutes(-10) });
		await _dbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		var recent = await _notificationQuery.GetOlderThan(currentTime);
		Assert.HasCount(1, recent);

		_dbContext.Notifications.RemoveRange(_dbContext.Notifications);
		await _dbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);
	}

	[TestMethod]
	public async Task RemoveAsyncTest()
	{
		var currentTime = DateTime.UtcNow;
		_dbContext.Notifications.Add(
			new NotificationItem { DateTime = DateTime.UtcNow.AddMinutes(-10) });
		await _dbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		var recent = await _notificationQuery.GetOlderThan(currentTime);
		Assert.HasCount(1, recent);

		await _notificationQuery.RemoveAsync(recent);

		var countAsync =
			await _dbContext.Notifications.CountAsync(TestContext.CancellationTokenSource.Token);
		Assert.AreEqual(0, countAsync);
	}

	[TestMethod]
	public async Task RemoveOlderThanAsync_RemovesOldItems()
	{
		// ExecuteDeleteAsync is not supported by the EF Core in-memory provider, so use SQLite here.
		using var connection = new SqliteConnection("Filename=:memory:");
		connection.Open();
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseSqlite(connection)
			.Options;
		var dbContext = new ApplicationDbContext(options);
		dbContext.Database.EnsureCreated();

		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(connection));
		var scopeFactory = services.BuildServiceProvider()
			.GetRequiredService<IServiceScopeFactory>();
		var sut = new NotificationQuery(dbContext, new FakeIWebLogger(), scopeFactory);

		var cutoff = DateTime.UtcNow;
		var epoch = ( ( DateTimeOffset ) cutoff.AddMinutes(-10) ).ToUnixTimeSeconds();
		dbContext.Notifications.Add(new NotificationItem
		{
			DateTime = cutoff.AddMinutes(-10), DateTimeEpoch = epoch
		});
		await dbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		var deleted = await sut.RemoveOlderThanAsync(cutoff);
		Assert.AreEqual(1, deleted);

		var countAsync =
			await dbContext.Notifications.CountAsync(TestContext.CancellationTokenSource.Token);
		Assert.AreEqual(0, countAsync);
	}

	private static IServiceScopeFactory CreateIsolatedScope(string testName)
	{
		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase(testName));
		var serviceProvider = services.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	[TestMethod]
	public async Task AddNotification_List_SmallContent_SingleNotification()
	{
		var scopeFactory = CreateIsolatedScope(nameof(AddNotification_List_SmallContent_SingleNotification));
		var scope = scopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var sut = new NotificationQuery(dbContext, new FakeIWebLogger(), scopeFactory);

		var items = new List<string> { "a", "b", "c" };
		await sut.AddNotification(
			new ApiNotificationResponseModel<List<string>>(items, ApiNotificationType.Welcome));

		var count = await dbContext.Notifications.CountAsync(TestContext.CancellationTokenSource.Token);
		Assert.AreEqual(1, count);
	}

	[TestMethod]
	public async Task AddNotification_List_LargeContent_SplitsIntoChunks()
	{
		var scopeFactory = CreateIsolatedScope(nameof(AddNotification_List_LargeContent_SplitsIntoChunks));
		var scope = scopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var sut = new NotificationQuery(dbContext, new FakeIWebLogger(), scopeFactory);

		// Build a list whose total serialized size exceeds MaxContentLength.
		// Each item is sized so that a modest number of items crosses the threshold.
		const int itemCount = 20;
		var itemSize = NotificationQuery.MaxContentLength / itemCount + 1000;
		var items = Enumerable.Range(0, itemCount)
			.Select(_ => new string('x', itemSize))
			.ToList();

		await sut.AddNotification(
			new ApiNotificationResponseModel<List<string>>(items, ApiNotificationType.Welcome));

		var count = await dbContext.Notifications.CountAsync(TestContext.CancellationTokenSource.Token);
		Assert.IsGreaterThan(1, count, "Expected multiple notifications when serialized content exceeds the size limit");
	}

	[TestMethod]
	public async Task AddNotification_List_SingleOversizedItem_LogsAndSkips()
	{
		var scopeFactory = CreateIsolatedScope(nameof(AddNotification_List_SingleOversizedItem_LogsAndSkips));
		var scope = scopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var logger = new FakeIWebLogger();
		var sut = new NotificationQuery(dbContext, logger, scopeFactory);

		// A single item whose serialized size exceeds MaxContentLength — cannot be split further
		var oversizedItem = new string('x', NotificationQuery.MaxContentLength + 1);
		var items = new List<string> { oversizedItem };

		var result = await sut.AddNotification(
			new ApiNotificationResponseModel<List<string>>(items, ApiNotificationType.Welcome));

		Assert.AreEqual(string.Empty, result.Content);
		Assert.Contains(log =>
				log.Item2?.Contains(NotificationQuery.ErrorMessageContentToLong) == true,
			logger.TrackedExceptions);
	}
}
