using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.database.Models;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public sealed class SynchronizeControllerTest
{
	[TestMethod]
	public async Task Index_Result_200()
	{
		var items =
			new Dictionary<string, FileIndexItem.ExifStatus>
			{
				{ "/", FileIndexItem.ExifStatus.Ok }
			};
		var actionResult = await new SynchronizeController(
			new FakeIManualBackgroundSyncService(items)
		).Index("/") as OkObjectResult;

		Assert.AreEqual(200, actionResult?.StatusCode);
	}

	[TestMethod]
	public async Task Index_Result_NotFound()
	{
		var items = new Dictionary<string, FileIndexItem.ExifStatus>
		{
			{ "/", FileIndexItem.ExifStatus.NotFoundNotInIndex }
		};
		var actionResult = await new SynchronizeController(
			new FakeIManualBackgroundSyncService(items)
		).Index("/") as NotFoundObjectResult;

		Assert.AreEqual(404, actionResult?.StatusCode);
	}

	[TestMethod]
	public async Task Index_Result_Waiting()
	{
		var items = new Dictionary<string, FileIndexItem.ExifStatus>
		{
			{ "/", FileIndexItem.ExifStatus.OperationNotSupported }
		};

		var actionResult = await new SynchronizeController(
			new FakeIManualBackgroundSyncService(items)
		).Index("/") as BadRequestObjectResult;

		Assert.AreEqual(400, actionResult?.StatusCode);
	}

	[TestMethod]
	public async Task Index_InvalidModel_Sync()
	{
		var controller = new SynchronizeController(
			new FakeIManualBackgroundSyncService(new Dictionary<string, FileIndexItem.ExifStatus>())
		);
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		controller.ModelState.AddModelError("Key", "ErrorMessage");
		var result = await controller.Index("Invalid");
		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}
}
