using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.import.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.import.Services;

[TestClass]
public class ImportThumbnailServiceTests
{
	private readonly AppSettings _appSettings;

	public ImportThumbnailServiceTests()
	{
		_appSettings = new AppSettings();
	}

	[TestMethod]
	public async Task WriteThumbnailsTest_ListShouldBeEq()
	{
		var sut = new ImportThumbnailService(new FakeSelectorStorage(), new FakeIWebLogger(),
			_appSettings);
		var result = await sut.WriteThumbnails(new List<string>(),
			new List<string> { "123" });
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task WriteThumbnailsTest_NotFound()
	{
		var logger = new FakeIWebLogger();
		var sut = new ImportThumbnailService(new FakeSelectorStorage(), logger,
			_appSettings);

		var result = await sut.WriteThumbnails(new List<string> { "123" },
			new List<string> { "123" });

		Assert.IsTrue(result);
		Assert.IsTrue(logger.TrackedInformation.FirstOrDefault().Item2?.Contains("not exist"));
	}

	[TestMethod]
	public async Task WriteThumbnailsTest_ShouldMoveFile()
	{
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage(new List<string>(),
			new List<string> { "/upload/123.jpg" });

		var sut = new ImportThumbnailService(new FakeSelectorStorage(storage), logger,
			_appSettings);

		await sut.WriteThumbnails(new List<string> { "/upload/123.jpg" },
			new List<string> { "123" });

		Assert.IsFalse(storage.ExistFile("/upload/123.jpg"));
		Assert.IsTrue(storage.ExistFile("123"));
	}

	[TestMethod]
	public void MapToTransferObject1()
	{
		var inputList = new List<string> { "12345678901234567890123456" };

		var sut = new ImportThumbnailService(new FakeSelectorStorage(), new FakeIWebLogger(),
			_appSettings);

		var result = sut.MapToTransferObject(inputList).ToList();

		Assert.AreEqual("12345678901234567890123456", result.FirstOrDefault()?.FileHash);
		Assert.IsTrue(result.FirstOrDefault()?.Large);
	}

	[TestMethod]
	public void MapToTransferObject1_2000()
	{
		var inputList = new List<string> { "12345678901234567890123456@2000" };
		var sut = new ImportThumbnailService(new FakeSelectorStorage(), new FakeIWebLogger(),
			_appSettings);
		var result = sut.MapToTransferObject(inputList).ToList();

		Assert.AreEqual("12345678901234567890123456", result.FirstOrDefault()?.FileHash);
		Assert.IsTrue(result.FirstOrDefault()?.ExtraLarge);
	}

	[TestMethod]
	public void MapToTransferObject1_NonValidType()
	{
		var inputList = new List<string> { "1234567890123456" };
		var sut = new ImportThumbnailService(new FakeSelectorStorage(), new FakeIWebLogger(),
			_appSettings);

		Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
			sut.MapToTransferObject(inputList));
	}
	
}
