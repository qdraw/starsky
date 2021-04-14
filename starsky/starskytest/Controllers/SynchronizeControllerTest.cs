using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.database.Models;
using starskytest.FakeMocks;

namespace starskytest.Controllers
{
	[TestClass]
	public class SynchronizeControllerTest
	{
		[TestMethod]
		public async Task Index_Result_200()
		{
			var items = new Dictionary<string, FileIndexItem.ExifStatus>{{"/", 
				FileIndexItem.ExifStatus.Ok }};
			var actionResult = await new SynchronizeController(
				new FakeIManualBackgroundSyncService(items)
				).Index("/") as OkObjectResult;
			
			Assert.AreEqual(200, actionResult.StatusCode);
		}
		
		[TestMethod]
		public async Task Index_Result_NotFound()
		{
			var items = new Dictionary<string, FileIndexItem.ExifStatus>{{"/", 
				FileIndexItem.ExifStatus.NotFoundNotInIndex }};
			var actionResult = await new SynchronizeController(
				new FakeIManualBackgroundSyncService(items)
			).Index("/") as NotFoundObjectResult;
			
			Assert.AreEqual(404, actionResult.StatusCode);
		}
		
		[TestMethod]
		public async Task Index_Result_Waiting()
		{
			var items = new Dictionary<string, FileIndexItem.ExifStatus>{{"/", 
				FileIndexItem.ExifStatus.OperationNotSupported }};
			
			var actionResult = await new SynchronizeController(
				new FakeIManualBackgroundSyncService(items)
			).Index("/") as BadRequestObjectResult;
			
			Assert.AreEqual(400, actionResult.StatusCode);
		}
	}
}
