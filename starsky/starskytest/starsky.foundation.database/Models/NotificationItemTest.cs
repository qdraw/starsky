using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;

namespace starskytest.starsky.foundation.database.Models
{
	[TestClass]
	public class NotificationItemTest
	{
		[TestMethod]
		public void NotificationItemIdTest()
		{
			var notificationItem = new NotificationItem();
			Assert.AreEqual(0, notificationItem.Id);
		}
	}
}


