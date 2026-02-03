using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.feature.rename.Models;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Models;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public class BatchRenameDateTimeControllerDatetimeRepairTest
{
	[TestMethod]
	public void PreviewDatetimeRepair_WithTimezoneRequest_ReturnsOk()
	{
		// Arrange
		var query = new FakeIQuery([
			new FileIndexItem("/test/20240313_011530_IMG_001.jpg")
			{
				FileName = "20240313_011530_IMG_001.jpg",
				ParentDirectory = "/test",
				Status = FileIndexItem.ExifStatus.Ok
			}
		]);
		var selectorStorage = new FakeSelectorStorage();
		var logger = new FakeIWebLogger();
		var controller =
			new BatchRenameDateTimeController(query, selectorStorage, logger, new AppSettings());

		var request = new FilenameDatetimeRepairRequest<ExifTimezoneBasedCorrectionRequest>
		{
			FilePaths = ["/test/20240313_011530_IMG_001.jpg"],
			Collections = true,
			CorrectionRequest = new ExifTimezoneBasedCorrectionRequest
			{
				RecordedTimezoneId = "UTC", CorrectTimezoneId = "Europe/Amsterdam"
			}
		};

		// Act
		var result = controller.PreviewTimezoneDatetimeRepair(request);

		// Assert
		Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
		var okResult = result.Result as OkObjectResult;
		var mappings = okResult?.Value as List<FilenameDatetimeRepairMapping>;
		Assert.IsNotNull(mappings);
		Assert.HasCount(1, mappings);
		Assert.IsFalse(mappings[0].HasError);
	}

	[TestMethod]
	public void PreviewDatetimeRepair_WithCustomOffsetRequest_ReturnsOk()
	{
		// Arrange
		var query = new FakeIQuery([
			new FileIndexItem("/test/20240313_011530_IMG_001.jpg")
			{
				FileName = "20240313_011530_IMG_001.jpg",
				ParentDirectory = "/test",
				Status = FileIndexItem.ExifStatus.Ok
			}
		]);
		var selectorStorage = new FakeSelectorStorage();
		var logger = new FakeIWebLogger();
		var controller =
			new BatchRenameDateTimeController(query, selectorStorage, logger, new AppSettings());

		var request = new FilenameDatetimeRepairRequest<ExifCustomOffsetCorrectionRequest>
		{
			FilePaths = ["/test/20240313_011530_IMG_001.jpg"],
			Collections = true,
			CorrectionRequest = new ExifCustomOffsetCorrectionRequest
			{
				Year = 0,
				Month = 0,
				Day = 0,
				Hour = 1,
				Minute = 0,
				Second = 0
			}
		};

		// Act
		var result = controller.PreviewCustomOffsetDatetimeRepair(request);

		// Assert
		Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
		var okResult = result.Result as OkObjectResult;
		var mappings = okResult?.Value as List<FilenameDatetimeRepairMapping>;
		Assert.IsNotNull(mappings);
		Assert.HasCount(1, mappings);
		Assert.IsFalse(mappings[0].HasError);
		Assert.AreEqual(1.0, mappings[0].OffsetHours);
	}

	[TestMethod]
	public void PreviewDatetimeRepair_NullCorrectionRequest_ReturnsBadRequest()
	{
		// Arrange
		var query = new FakeIQuery();
		var selectorStorage = new FakeSelectorStorage();
		var logger = new FakeIWebLogger();
		var controller =
			new BatchRenameDateTimeController(query, selectorStorage, logger, new AppSettings());

		var request = new FilenameDatetimeRepairRequest<ExifTimezoneBasedCorrectionRequest>
		{
			FilePaths = ["/test/image.jpg"], Collections = true, CorrectionRequest = null!
		};

		// Act
		var result = controller.PreviewTimezoneDatetimeRepair(request);

		// Assert
		Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
	}

	[TestMethod]
	public async Task ExecuteDatetimeRepairAsync_WithTimezoneRequest_ReturnsOk()
	{
		// Arrange
		var query = new FakeIQuery([
			new FileIndexItem("/test/20240313_011530_IMG_001.jpg")
			{
				FileName = "20240313_011530_IMG_001.jpg",
				ParentDirectory = "/test",
				Status = FileIndexItem.ExifStatus.Ok
			}
		]);
		var selectorStorage = new FakeSelectorStorage(new FakeIStorage([
			"/test/20240313_011530_IMG_001.jpg"
		]));
		var logger = new FakeIWebLogger();
		var controller =
			new BatchRenameDateTimeController(query, selectorStorage, logger, new AppSettings());

		var request = new FilenameDatetimeRepairRequest<ExifTimezoneBasedCorrectionRequest>
		{
			FilePaths = ["/test/20240313_011530_IMG_001.jpg"],
			Collections = true,
			CorrectionRequest = new ExifTimezoneBasedCorrectionRequest
			{
				RecordedTimezoneId = "UTC", CorrectTimezoneId = "Europe/Amsterdam"
			}
		};

		// Act
		var result = await controller.ExecuteTimezoneDatetimeRepairAsync(request);

		// Assert
		Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
		var okResult = result.Result as OkObjectResult;
		var fileItems = okResult?.Value as List<FileIndexItem>;
		Assert.IsNotNull(fileItems);
		Assert.HasCount(1,
			fileItems.Where(p
				=> p.Status == FileIndexItem.ExifStatus.Ok).ToList());
		Assert.HasCount(1,
			fileItems.Where(p
				=> p.Status == FileIndexItem.ExifStatus.Deleted).ToList());
	}

	[TestMethod]
	public async Task ExecuteDatetimeRepairAsync_WithCustomOffsetRequest_ReturnsOk()
	{
		// Arrange
		var query = new FakeIQuery([
			new FileIndexItem("/test/20240313_011530_IMG_001.jpg")
			{
				FileName = "20240313_011530_IMG_001.jpg",
				ParentDirectory = "/test",
				Status = FileIndexItem.ExifStatus.Ok
			}
		]);
		var selectorStorage = new FakeSelectorStorage(new FakeIStorage([
			"/test/20240313_011530_IMG_001.jpg"
		]));
		var logger = new FakeIWebLogger();
		var controller =
			new BatchRenameDateTimeController(query, selectorStorage, logger, new AppSettings());

		var request = new FilenameDatetimeRepairRequest<ExifCustomOffsetCorrectionRequest>
		{
			FilePaths = ["/test/20240313_011530_IMG_001.jpg"],
			Collections = true,
			CorrectionRequest = new ExifCustomOffsetCorrectionRequest
			{
				Year = 0,
				Month = 0,
				Day = 0,
				Hour = 1,
				Minute = 0,
				Second = 0
			}
		};

		// Act
		var result = await controller.ExecuteCustomOffsetDatetimeRepairAsync(request);

		// Assert
		Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
		var okResult = result.Result as OkObjectResult;
		var fileItems = okResult?.Value as List<FileIndexItem>;
		Assert.IsNotNull(fileItems);
		Assert.HasCount(1,
			fileItems.Where(p
				=> p.Status == FileIndexItem.ExifStatus.Ok).ToList());
		Assert.HasCount(1,
			fileItems.Where(p
				=> p.Status == FileIndexItem.ExifStatus.Deleted).ToList());
		Assert.AreEqual("20240313_021530_IMG_001.jpg",
			fileItems.FirstOrDefault(p =>
				p.Status == FileIndexItem.ExifStatus.Ok)?.FileName);
		Assert.AreEqual("20240313_011530_IMG_001.jpg",
			fileItems.FirstOrDefault(p =>
				p.Status == FileIndexItem.ExifStatus.Deleted)?.FileName);
	}

	[TestMethod]
	public async Task ExecuteDatetimeRepairAsync_NullCorrectionRequest_ReturnsBadRequest()
	{
		// Arrange
		var query = new FakeIQuery();
		var selectorStorage = new FakeSelectorStorage();
		var logger = new FakeIWebLogger();
		var controller =
			new BatchRenameDateTimeController(query, selectorStorage, logger, new AppSettings());

		var request = new FilenameDatetimeRepairRequest<ExifCustomOffsetCorrectionRequest>
		{
			FilePaths = ["/test/image.jpg"], Collections = true, CorrectionRequest = null!
		};

		// Act
		var result = await controller.ExecuteCustomOffsetDatetimeRepairAsync(request);

		// Assert
		Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
	}

	private static BatchRenameDateTimeController CreateController()
	{
		return new BatchRenameDateTimeController(
			new FakeIQuery(),
			new FakeSelectorStorage(),
			new FakeIWebLogger(),
			new AppSettings()
		);
	}

	[TestMethod]
	public async Task ExecuteDatetimeRepairAsync_MultipleFiles_ProcessesAll()
	{
		// Arrange
		var query = new FakeIQuery([
			new FileIndexItem("/test/20240313_011530_IMG_001.jpg")
			{
				FileName = "20240313_011530_IMG_001.jpg",
				ParentDirectory = "/test",
				Status = FileIndexItem.ExifStatus.Ok
			},

			new FileIndexItem("/test/20240313_021530_IMG_002.jpg")
			{
				FileName = "20240313_021530_IMG_002.jpg",
				ParentDirectory = "/test",
				Status = FileIndexItem.ExifStatus.Ok
			}
		]);
		var selectorStorage = new FakeSelectorStorage(new FakeIStorage([
			"/test/20240313_011530_IMG_001.jpg", "/test/20240313_021530_IMG_002.jpg"
		]));
		var logger = new FakeIWebLogger();
		var controller =
			new BatchRenameDateTimeController(query, selectorStorage, logger, new AppSettings());

		var request = new FilenameDatetimeRepairRequest<ExifCustomOffsetCorrectionRequest>
		{
			FilePaths =
				["/test/20240313_011530_IMG_001.jpg", "/test/20240313_021530_IMG_002.jpg"],
			Collections = false,
			CorrectionRequest = new ExifCustomOffsetCorrectionRequest { Hour = 1 }
		};

		// Act
		var result = await controller.ExecuteCustomOffsetDatetimeRepairAsync(request);

		// Assert
		Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
		var okResult = result.Result as OkObjectResult;
		var fileItems = okResult?.Value as List<FileIndexItem>;
		Assert.IsNotNull(fileItems);
		Assert.HasCount(2,
			fileItems.Where(p
				=> p.Status == FileIndexItem.ExifStatus.Ok).ToList());
		Assert.HasCount(2,
			fileItems.Where(p
				=> p.Status == FileIndexItem.ExifStatus.Deleted).ToList());
	}

	[TestMethod]
	public void PreviewCustomOffsetDatetimeRepair_ModelStateInvalid_ReturnsBadRequest()
	{
		var controller = CreateController();
		controller.ModelState.AddModelError("FilePaths", "Required");
		var request = new FilenameDatetimeRepairRequest<ExifCustomOffsetCorrectionRequest>
		{
			FilePaths = ["/test/file.jpg"],
			CorrectionRequest = new ExifCustomOffsetCorrectionRequest(),
			Collections = false
		};
		var result = controller.PreviewCustomOffsetDatetimeRepair(request);
		Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
		var badRequestResult = result.Result as BadRequestObjectResult;
		Assert.AreEqual("Model is not valid", badRequestResult!.Value);
	}

	[TestMethod]
	public void PreviewCustomOffsetDatetimeRepair_CorrectionRequestNull_ReturnsBadRequest()
	{
		var controller = CreateController();
		var request = new FilenameDatetimeRepairRequest<ExifCustomOffsetCorrectionRequest>
		{
			FilePaths = ["/test/file.jpg"], CorrectionRequest = null!, Collections = false
		};
		var result = controller.PreviewCustomOffsetDatetimeRepair(request);
		Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
		var badRequestResult = result.Result as BadRequestObjectResult;
		Assert.AreEqual("Model is not valid", badRequestResult!.Value);
	}

	[TestMethod]
	public void PreviewCustomOffsetDatetimeRepair_InvalidModel()
	{
		var controller = CreateController();
		var request = new FilenameDatetimeRepairRequest<ExifCustomOffsetCorrectionRequest>
		{
			FilePaths = ["/test/file.jpg"],
			CorrectionRequest = new ExifCustomOffsetCorrectionRequest(),
			Collections = false
		};
		var result = controller.PreviewCustomOffsetDatetimeRepair(request);
		Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
		var badRequestResult = result.Result as BadRequestObjectResult;
		Assert.AreEqual("Model is not valid", badRequestResult!.Value);
	}
	
	[TestMethod]
	public async Task ExecuteTimezoneDatetimeRepairAsync_InvalidModel()
	{
		var controller = CreateController();
		var request = new FilenameDatetimeRepairRequest<ExifTimezoneBasedCorrectionRequest>
		{
			FilePaths = ["/test/file.jpg"],
			CorrectionRequest = null!,
			Collections = false
		};
		var result = await controller.ExecuteTimezoneDatetimeRepairAsync(request);
		Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
		var badRequestResult = result.Result as BadRequestObjectResult;
		Assert.AreEqual("Model is not valid", badRequestResult!.Value);
	}
}
