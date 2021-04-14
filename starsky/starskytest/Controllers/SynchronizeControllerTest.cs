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
			var actionResult = await new SynchronizeController(new FakeISynchronize(), new FakeIQuery(
				new List<FileIndexItem>
				{
					new FileIndexItem("/"){IsDirectory = true}
					
				}), new FakeIWebSocketConnectionsService(), 
				new FakeIBackgroundTaskQueue(), 
				new FakeMemoryCache(null)).Index("/") as OkObjectResult;
			
			Assert.AreEqual(200, actionResult.StatusCode);
		}
		
		[TestMethod]
		public async Task Index_Result_NotFound()
		{
			var actionResult = await new SynchronizeController(new FakeISynchronize(), new FakeIQuery(
					new List<FileIndexItem>()
					), new FakeIWebSocketConnectionsService(), 
				new FakeIBackgroundTaskQueue(), 
				new FakeMemoryCache(null)).Index("/") as NotFoundObjectResult;

			Assert.AreEqual(404, actionResult.StatusCode);
		}
	}
}
