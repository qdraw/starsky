using System;
using System.Threading.Tasks;
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

namespace starskytest.starsky.foundation.database.NotificationsTest
{
	/// <summary>
	/// NotificationQuery
	/// </summary>
	[TestClass]
	public sealed class NotificationQueryTest
	{
		private readonly NotificationQuery _notificationQuery;
		private readonly FakeIWebLogger _logger;
		private readonly ApplicationDbContext _dbContext;

		public NotificationQueryTest()
		{
			var serviceScope = CreateNewScope();
			var scope = serviceScope.CreateScope();
			_dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var serviceScopeFactory = scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
			_logger = new FakeIWebLogger();
			_notificationQuery = new NotificationQuery(_dbContext, new FakeIWebLogger(), serviceScopeFactory);
		}
		
		private static IServiceScopeFactory CreateNewScope()
		{
			var services = new ServiceCollection();
			services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nameof(NotificationQueryTest)));
			var serviceProvider = services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}

		[TestMethod]
		public async Task ShouldContainWhenAdd()
		{
			await _notificationQuery.AddNotification(
				new ApiNotificationResponseModel<string>("test"){Type = ApiNotificationType.Welcome});
			
			var testNotification =
				await _dbContext.Notifications.FirstOrDefaultAsync(p =>
					p.Content!.Contains("test"));
			
			Assert.IsNotNull(testNotification);
			
			_dbContext.Notifications.Remove(testNotification);
			await _dbContext.SaveChangesAsync();
		}

		[TestMethod]
		public async Task Disposed()
		{
			var serviceScopeFactory = CreateNewScope();
			var scope = serviceScopeFactory.CreateScope();
			var dbContextDisposed = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			
			// Dispose here
			await dbContextDisposed.DisposeAsync();
			
			await new NotificationQuery(dbContextDisposed, _logger, serviceScopeFactory).AddNotification(
				new ApiNotificationResponseModel<string>("test_disposed_notification"){Type = ApiNotificationType.Welcome});
			
			var context = new InjectServiceScope(serviceScopeFactory).Context();
			
			var testNotification =
				await context.Notifications.FirstOrDefaultAsync(p =>
					p.Content!.Contains("test_disposed_notification"));

			// and remove it afterwards
			foreach ( var notificationsItem in await context.Notifications.ToListAsync() )
			{
				context.Notifications.Remove(notificationsItem);
			}
			await context.SaveChangesAsync();
			
			Assert.IsNotNull(testNotification?.Content);
			Assert.IsNotNull(testNotification.DateTimeEpoch);
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
			
			var unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds() -1;

			Assert.IsTrue(unixTime <= item.DateTimeEpoch);
			
			var recent = await _notificationQuery.GetNewerThan(currentTime);
			Assert.AreEqual(1, recent.Count);
			
			_dbContext.Notifications.RemoveRange(recent);
			await _dbContext.SaveChangesAsync();
		}
		
		[TestMethod]
		public async Task GetNewerThan_RecentItems_OldIgnored()
		{
			var currentTime = DateTime.UtcNow;
			_dbContext.Notifications.Add(
				new NotificationItem() {DateTime = DateTime.UnixEpoch});
			await _dbContext.SaveChangesAsync();
			
			var recent = await _notificationQuery.GetNewerThan(currentTime);
			Assert.AreEqual(0, recent.Count);
			
			_dbContext.Notifications.RemoveRange(_dbContext.Notifications);
			await _dbContext.SaveChangesAsync();
		}
		
		[TestMethod]
		public async Task Get_OlderItems()
		{
			var currentTime = DateTime.UtcNow;
			_dbContext.Notifications.Add(
				new NotificationItem() {DateTime = DateTime.UtcNow.AddMinutes(-10)});
			await _dbContext.SaveChangesAsync();
			
			var recent = await _notificationQuery.GetOlderThan(currentTime);
			Assert.AreEqual(1, recent.Count);
			
			_dbContext.Notifications.RemoveRange(_dbContext.Notifications);
			await _dbContext.SaveChangesAsync();
		}
		
		[TestMethod]
		public async Task RemoveAsyncTest()
		{
			var currentTime = DateTime.UtcNow;
			_dbContext.Notifications.Add(
				new NotificationItem() {DateTime = DateTime.UtcNow.AddMinutes(-10)});
			await _dbContext.SaveChangesAsync();
			
			var recent = await _notificationQuery.GetOlderThan(currentTime);
			Assert.AreEqual(1, recent.Count);

			await _notificationQuery.RemoveAsync(recent);
			
			var countAsync = await _dbContext.Notifications.CountAsync();
			Assert.AreEqual(0,countAsync);
		}
	}
}
