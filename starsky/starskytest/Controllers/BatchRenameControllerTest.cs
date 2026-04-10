using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.feature.rename.Models;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.Controllers;

[TestClass]
public class BatchRenameControllerTest
{
	[TestMethod]
	public void PreviewBatchRename_ReturnsOk_WithValidRequest()
	{
		// Arrange
		var query = new FakeIQuery();
		var selectorStorage = new FakeSelectorStorage();
		var logger = new FakeIWebLogger();
		var controller =
			new BatchRenameController(query, selectorStorage, logger, new AppSettings());
		var request = new BatchRenameRequest
		{
			FilePaths = ["/test.jpg"],
			Pattern = "{yyyy}{MM}{dd}_{filenamebase}.{ext}"
		};

		// Act
		var result = controller.PreviewBatchRename(request);

		// Assert
		Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
	}

	[TestMethod]
	public void PreviewBatchRename_ReturnsBadRequest_WhenPatternIsNull()
	{
		// Arrange
		var query = new FakeIQuery();
		var selectorStorage = new FakeSelectorStorage();
		var logger = new FakeIWebLogger();
		var controller =
			new BatchRenameController(query, selectorStorage, logger, new AppSettings());
		var request = new BatchRenameRequest { FilePaths = ["/test.jpg"], Pattern = null! };

		// Act
		var result = controller.PreviewBatchRename(request);

		// Assert
		Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
	}

	[TestMethod]
	public async Task ExecuteBatchRenameAsync_ReturnsOk_WithValidRequest()
	{
		// Arrange
		var query = new FakeIQuery();
		var selectorStorage = new FakeSelectorStorage();
		var logger = new FakeIWebLogger();
		var controller =
			new BatchRenameController(query, selectorStorage, logger, new AppSettings());
		var request = new BatchRenameRequest
		{
			FilePaths = ["/test.jpg"],
			Pattern = "{yyyy}{MM}{dd}_{filenamebase}.{ext}",
			Collections = false
		};

		// Act
		var result = await controller.ExecuteBatchRenameAsync(request);

		// Assert
		Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
	}
}
