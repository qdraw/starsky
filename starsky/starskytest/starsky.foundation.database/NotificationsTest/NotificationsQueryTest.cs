using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Notifications;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.NotificationsTest
{
	/// <summary>
	/// NotificationQuery
	/// </summary>
	[TestClass]
	public class NotificationsQueryTest
	{
		private readonly NotificationQuery _notificationQuery;
		private readonly FakeIWebLogger _logger;
		private readonly ApplicationDbContext _dbContext;

		public NotificationsQueryTest()
		{
			var serviceScope = CreateNewScope();
			var scope = serviceScope.CreateScope();
			_dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			_logger = new FakeIWebLogger();
			_notificationQuery = new NotificationQuery(_dbContext);
		}
		private IServiceScopeFactory CreateNewScope()
		{
			var services = new ServiceCollection();
			services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nameof(NotificationsQueryTest)));
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
					p.Content.Contains("test"));
			
			Assert.IsNotNull(testNotification);
			
			_dbContext.Notifications.Remove(testNotification);
			await _dbContext.SaveChangesAsync();
		}

		[TestMethod]
		public async Task Get_RecentItems_Ok()
		{
			var currentTime = DateTime.UtcNow;
			await _notificationQuery.AddNotification(
				new ApiNotificationResponseModel<string>("test")
				{
					Type = ApiNotificationType.Welcome
				});

			var recent = await _notificationQuery.GetNewerThan(currentTime);
			Assert.AreEqual(1, recent.Count);
			
			_dbContext.Notifications.RemoveRange(recent);
			await _dbContext.SaveChangesAsync();
		}
		
		[TestMethod]
		public async Task Get_RecentItems_OldIgnored()
		{
			var currentTime = DateTime.UtcNow;
			_dbContext.Notifications.Add(
				new NotificationItem() {DateTime = DateTime.UnixEpoch});
			await _dbContext.SaveChangesAsync();
			
			var recent = await _notificationQuery.GetNewerThan(currentTime);
			Assert.AreEqual(0, recent.Count);
		}
	}
}
