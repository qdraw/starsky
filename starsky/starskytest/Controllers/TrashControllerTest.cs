using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.feature.trash.Services;
using starsky.foundation.database.Models;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public class TrashControllerTest
{
	[TestMethod]
	public async Task TrashControllerTest_BadInput()
	{
		var controller = new TrashController(
			new FakeIMoveToTrashService(new List<FileIndexItem>()));
		var result = await controller.TrashMoveAsync(null, true) as BadRequestObjectResult;
		Assert.AreEqual(400,result?.StatusCode);
	}
	
	[TestMethod]
	public async Task TrashControllerTest_Index()
	{
		var controller = new TrashController(
			new FakeIMoveToTrashService(new List<FileIndexItem>()));
		var result = await controller.TrashMoveAsync("/test.jpg", true) as JsonResult;
		var resultValue = result?.Value as List<FileIndexItem>;
		
		Assert.AreEqual(1, resultValue?.Count);
	}
}
