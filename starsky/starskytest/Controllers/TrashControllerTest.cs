using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
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
	public async Task TrashControllerTest_NotFound()
	{
		var controller = new TrashController(
			new FakeIMoveToTrashService(new List<FileIndexItem>()));
		var result = await controller.TrashMoveAsync("/test.jpg", true) as JsonResult;
		var resultValue = result?.Value as List<FileIndexItem>;
		
		Assert.AreEqual(1, resultValue?.Count);
	}
	
	[TestMethod]
	public async Task TrashControllerTest_Ok()
	{
		var controller = new TrashController(
			new FakeIMoveToTrashService(new List<FileIndexItem>{new FileIndexItem("/test.jpg")
			{
				Status = FileIndexItem.ExifStatus.Ok
			}}));
		var result = await controller.TrashMoveAsync("/test.jpg", true) as JsonResult;
		var resultValue = result?.Value as List<FileIndexItem>;
		
		Assert.AreEqual(1, resultValue?.Count);
	}
	
	[TestMethod]
	public void DetectToUseSystemTrash_Ok()
	{
		var controller = new TrashController(
			new FakeIMoveToTrashService(new List<FileIndexItem>()));
		
		var result = controller.DetectToUseSystemTrash() as JsonResult;
		
		bool.TryParse(result?.Value?.ToString(), out var resultValue);
		
		Assert.AreEqual(true, resultValue);
	}
}
