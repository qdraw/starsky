using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.database.Models;
using starskytest.FakeMocks;

namespace starskytest.Controllers
{
	[TestClass]
	public class NotificationControllerTest
	{
		[TestMethod]
		public async Task NotificationController_Get_Test_Null()
		{
			var notificationController = new NotificationController(new FakeINotificationQuery());
			var result = await notificationController.GetNotifications(null) as BadRequestObjectResult;
			Assert.IsNotNull(result);
			Assert.AreEqual(400, result.StatusCode);
		}
		
		[TestMethod]
		public async Task NotificationController_Get_Test_LongerThan1Day()
		{
			var notificationController = new NotificationController(new FakeINotificationQuery());
			var result = await notificationController.GetNotifications("2020-04-11T17:55:35.922319Z") as BadRequestObjectResult;
			Assert.IsNotNull(result);
			Assert.AreEqual(400, result.StatusCode);
		}
		
				
		[TestMethod]
		public async Task NotificationController_Get_Test_Now_HappyFlow()
		{
			var notificationController = new NotificationController(new FakeINotificationQuery(new List<NotificationItem>{new NotificationItem()
			{
				DateTime = DateTime.UtcNow
			}}));

			var dateTime = DateTime.UtcNow.AddMinutes(-1)
				.ToString(CultureInfo.InvariantCulture);
			var result = await notificationController.GetNotifications(dateTime) as JsonResult;
			Assert.IsNotNull(result);
			var parsedResult = result.Value as List<NotificationItem>;
			Assert.IsNotNull(parsedResult);
			Assert.AreEqual(1, parsedResult.Count);
		}

		[TestMethod]
		public void ParseDate1()
		{
			var (parsed, parsedDateTime) = NotificationController.ParseDate("2020-04-11T17:55:35.922319Z");
			Assert.IsTrue(parsed);
			var expected = new DateTime(2020, 4, 11, 17, 55, 35, 922).ToString(CultureInfo.InvariantCulture);
			Assert.AreEqual(expected,parsedDateTime.ToString(CultureInfo.InvariantCulture));
		}
		
		[TestMethod]
		public void ParseDateNonValid()
		{
			var (parsed,_) = NotificationController.ParseDate("non-valid");
			Assert.IsFalse(parsed);
		}
		
		[TestMethod]
		public void ParseDateInt()
		{
			var (parsed, parsedDateTime) = NotificationController.ParseDate("2");
			Assert.IsTrue(parsed);
			var expected = DateTime.UtcNow.AddHours(-2).ToString(CultureInfo.InvariantCulture);
			Assert.AreEqual(expected,parsedDateTime.ToString(CultureInfo.InvariantCulture));
		}
	}
	
}

