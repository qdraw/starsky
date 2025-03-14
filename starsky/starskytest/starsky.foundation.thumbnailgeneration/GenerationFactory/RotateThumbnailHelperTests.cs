using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetadataExtractor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.ReadMetaHelpers;
using starsky.foundation.thumbnailgeneration.GenerationFactory;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory;

[TestClass]
public class RotateThumbnailHelperTests
{
	private readonly FakeIStorage _fakeIStorage;
	private readonly FakeIWebLogger _logger;
	private readonly RotateThumbnailHelper _sut;

	public RotateThumbnailHelperTests()
	{
		_fakeIStorage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "corrupt", "test" },
			new List<byte[]?> { Array.Empty<byte>(), CreateAnImage.Bytes.ToArray() });
		var selectorStorage = new FakeSelectorStorage(_fakeIStorage);
		var appSettings = new AppSettings();
		_logger = new FakeIWebLogger();
		_sut = new RotateThumbnailHelper(selectorStorage, appSettings, _logger);
	}

	[TestMethod]
	public async Task RotateThumbnail_FileDoesNotExist()
	{
		var result = await _sut.RotateThumbnail("nonexistentFileHash", 1);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task RotateThumbnail_Success()
	{
		var result = await _sut.RotateThumbnail("test", 1, 100);

		Assert.IsTrue(result);
		Assert.IsTrue(_fakeIStorage.ExistFile("test"));

		var meta = ImageMetadataReader.ReadMetadata(
			_fakeIStorage.ReadStream("test")).ToList();

		// asserts
		Assert.AreEqual(67, ReadMetaExif.GetImageWidthHeight(meta, true));
		Assert.AreEqual(100, ReadMetaExif.GetImageWidthHeight(meta, false));
	}

	[TestMethod]
	public async Task RotateThumbnail_Exception()
	{
		var result = await _sut.RotateThumbnail("corrupt", 1);

		Assert.IsFalse(result);
		Assert.IsTrue(
			_logger.TrackedExceptions.Any(log =>
				log.Item2?.Contains("[RotateThumbnailHelper]") == true));
	}
}
