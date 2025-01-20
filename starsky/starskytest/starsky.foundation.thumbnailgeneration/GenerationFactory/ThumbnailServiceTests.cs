using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory;
using starsky.foundation.thumbnailgeneration.Interfaces;
using starsky.foundation.thumbnailgeneration.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory;

[TestClass]
public sealed class ThumbnailServiceTests
{
	private readonly AppSettings _appSettings;
	private readonly string _fakeIStorageImageSubPath;

	private readonly IUpdateStatusGeneratedThumbnailService
		_fakeIUpdateStatusGeneratedThumbnailService;

	private readonly ThumbnailImageFormat _imageFormat;

	private readonly FakeSelectorStorage _selectorStorage;

	public ThumbnailServiceTests()
	{
		_fakeIStorageImageSubPath = "/test.jpg";

		var iStorage = new FakeIStorage(new List<string> { "/" },
			new List<string> { _fakeIStorageImageSubPath },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });
		_selectorStorage = new FakeSelectorStorage(iStorage);
		_appSettings = new AppSettings();
		_fakeIUpdateStatusGeneratedThumbnailService =
			new FakeIUpdateStatusGeneratedThumbnailService();
		_imageFormat = new AppSettings().ThumbnailImageFormat;
	}

	[TestMethod]
	public async Task CreateThumbTest_FileHash_FileHashNull()
	{
		// Arrange
		var sut = new ThumbnailService(_selectorStorage, new FakeIWebLogger(),
			_appSettings, _fakeIUpdateStatusGeneratedThumbnailService, new FakeIVideoProcess(),
			new FileHashSubPathStorage(new FakeSelectorStorage(), new FakeIWebLogger()));

		// Act & Assert
		var resultModels = await sut.GenerateThumbnail("/notfound.jpg", null!);

		Assert.IsFalse(resultModels.FirstOrDefault()!.Success);
	}

	[TestMethod]
	public async Task CreateThumbTest_FileHash_ImageSubPathNotFound()
	{
		var sut = new ThumbnailService(_selectorStorage, new FakeIWebLogger(),
			_appSettings, _fakeIUpdateStatusGeneratedThumbnailService, new FakeIVideoProcess(),
			new FileHashSubPathStorage(new FakeSelectorStorage(), new FakeIWebLogger()));

		var isCreated =
			await sut.GenerateThumbnail(
				"/notfound.jpg", _fakeIStorageImageSubPath);

		Assert.IsFalse(isCreated.FirstOrDefault()!.Success);
	}

	[TestMethod]
	public async Task CreateThumbTest_FileHash_WrongImageType()
	{
		var sut = new ThumbnailService(_selectorStorage, new FakeIWebLogger(),
			_appSettings, _fakeIUpdateStatusGeneratedThumbnailService, new FakeIVideoProcess(),
			new FileHashSubPathStorage(new FakeSelectorStorage(), new FakeIWebLogger()));

		var isCreated = await sut.GenerateThumbnail(
			"/notfound.dng", _fakeIStorageImageSubPath);

		Assert.IsFalse(isCreated.FirstOrDefault()!.Success);
	}


	[TestMethod]
	[DataRow(true)]
	[DataRow(false)]
	public async Task CreateThumbTest_FileHash_IncludeOrSkipExtraLarge(bool includeExtraLarge)
	{
		var storage = new FakeIStorage(["/"],
			[_fakeIStorageImageSubPath],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		const string fileHash = "test_hash";
		var sut = new ThumbnailService(new FakeSelectorStorage(storage), new FakeIWebLogger(),
			_appSettings, _fakeIUpdateStatusGeneratedThumbnailService, new FakeIVideoProcess(),
			new FileHashSubPathStorage(new FakeSelectorStorage(storage), new FakeIWebLogger()));

		var isCreated =
			await sut.GenerateThumbnail(_fakeIStorageImageSubPath, fileHash, includeExtraLarge);

		Assert.IsTrue(isCreated.FirstOrDefault()!.Success);
		Assert.IsTrue(storage.ExistFile(
			ThumbnailNameHelper.Combine(fileHash, ThumbnailSize.Large, _imageFormat)));
		Assert.IsTrue(storage.ExistFile(
			ThumbnailNameHelper.Combine(fileHash, ThumbnailSize.Small, _imageFormat)));

		// depend on includeExtraLarge
		Assert.AreEqual(!includeExtraLarge, storage.ExistFile(
			ThumbnailNameHelper.Combine(fileHash, ThumbnailSize.ExtraLarge, _imageFormat)));
	}

	[TestMethod]
	public async Task CreateThumbTest_1arg_ThumbnailAlreadyExist()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { _fakeIStorageImageSubPath },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var hash =
			( await new FileHash(storage, new FakeIWebLogger()).GetHashCodeAsync(
				_fakeIStorageImageSubPath) )
			.Key;
		await storage.WriteStreamAsync(
			StringToStreamHelper.StringToStream("not 0 bytes"),
			ThumbnailNameHelper.Combine(hash, ThumbnailSize.ExtraLarge, _imageFormat));
		await storage.WriteStreamAsync(
			StringToStreamHelper.StringToStream("not 0 bytes"),
			ThumbnailNameHelper.Combine(hash, ThumbnailSize.Large, _imageFormat));
		await storage.WriteStreamAsync(
			StringToStreamHelper.StringToStream("not 0 bytes"),
			ThumbnailNameHelper.Combine(hash, ThumbnailSize.Small, _imageFormat));

		var sut = new ThumbnailService(new FakeSelectorStorage(storage), new FakeIWebLogger(),
			_appSettings, _fakeIUpdateStatusGeneratedThumbnailService, new FakeIVideoProcess(),
			new FileHashSubPathStorage(new FakeSelectorStorage(storage), new FakeIWebLogger()));

		var isCreated =
			await sut.GenerateThumbnail(_fakeIStorageImageSubPath); // 1 arg

		Assert.IsTrue(isCreated[0].Success);
	}

	[TestMethod]
	public async Task CreateThumbTest_1arg_Folder()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { _fakeIStorageImageSubPath },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var sut = new ThumbnailService(new FakeSelectorStorage(storage), new FakeIWebLogger(),
			_appSettings, _fakeIUpdateStatusGeneratedThumbnailService, new FakeIVideoProcess(),
			new FileHashSubPathStorage(new FakeSelectorStorage(storage), new FakeIWebLogger()));

		var isCreated = await sut.GenerateThumbnail("/");

		Assert.IsTrue(isCreated[0].Success);
	}

	[TestMethod]
	public async Task CreateThumbTest_NullFail()
	{
		var storage = new FakeIStorage(new List<string> { "/test" },
			new List<string> { "/test/test.jpg" },
			new List<byte[]?> { null });

		var sut = new ThumbnailService(new FakeSelectorStorage(storage), new FakeIWebLogger(),
			_appSettings, _fakeIUpdateStatusGeneratedThumbnailService, new FakeIVideoProcess(),
			new FileHashSubPathStorage(new FakeSelectorStorage(storage), new FakeIWebLogger()));

		var isCreated = await sut.GenerateThumbnail("/test/test.jpg");

		Assert.AreEqual(0, isCreated.Count);
	}

	[TestMethod]
	public async Task RotateThumbnail_NotFound()
	{
		var sut = new ThumbnailService(new FakeSelectorStorage(), new FakeIWebLogger(),
			_appSettings, _fakeIUpdateStatusGeneratedThumbnailService, new FakeIVideoProcess(),
			new FileHashSubPathStorage(new FakeSelectorStorage(), new FakeIWebLogger()));

		var result = await sut.RotateThumbnail("not-found", 0, 3);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task RotateThumbnail_Rotate()
	{
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var sut = new ThumbnailService(new FakeSelectorStorage(storage), new FakeIWebLogger(),
			_appSettings, _fakeIUpdateStatusGeneratedThumbnailService, new FakeIVideoProcess(),
			new FileHashSubPathStorage(new FakeSelectorStorage(storage), new FakeIWebLogger()));
		var result = await sut.RotateThumbnail("/test.jpg", -1, 3);

		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task RotateThumbnail_Corrupt()
	{
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "test" },
			new List<byte[]> { Array.Empty<byte>() });

		var sut = new ThumbnailService(new FakeSelectorStorage(storage), new FakeIWebLogger(),
			_appSettings, _fakeIUpdateStatusGeneratedThumbnailService, new FakeIVideoProcess(),
			new FileHashSubPathStorage(new FakeSelectorStorage(storage), new FakeIWebLogger()));

		var result = await sut.RotateThumbnail("/test.jpg", -1, 3);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task NotFound()
	{
		var sut = new ThumbnailService(new FakeSelectorStorage(),
			new FakeIWebLogger(), new AppSettings(),
			new UpdateStatusGeneratedThumbnailService(new FakeIThumbnailQuery()),
			new FakeIVideoProcess(),
			new FileHashSubPathStorage(new FakeSelectorStorage(), new FakeIWebLogger()));
		var resultModels = await sut.GenerateThumbnail("/not-found");

		Assert.IsFalse(resultModels.FirstOrDefault()!.Success);
	}

	[TestMethod]
	public async Task NotFound2()
	{
		var sut = new ThumbnailService(new FakeSelectorStorage(),
			new FakeIWebLogger(), new AppSettings(),
			new UpdateStatusGeneratedThumbnailService(new FakeIThumbnailQuery()),
			new FakeIVideoProcess(),
			new FileHashSubPathStorage(new FakeSelectorStorage(), new FakeIWebLogger()));
		var (stream, resultModels) = await sut.GenerateThumbnail("/not-found",
			"hash", ThumbnailImageFormat.unknown, ThumbnailSize.Large);

		Assert.IsNull(stream);
		Assert.IsFalse(resultModels.Success);
	}

	[TestMethod]
	public async Task GenerateThumbnail_NotFound()
	{
		var storage = new FakeSelectorStorage(new FakeIStorage([],
			["/not-found.jpg"], [[.. CreateAnImage.Bytes]]));

		var hashService = new FakeIFileHashSubPathStorage([( "/test.jpg", "hash", false )]);
		var sut = new ThumbnailService(storage,
			new FakeIWebLogger(), new AppSettings(),
			new UpdateStatusGeneratedThumbnailService(new FakeIThumbnailQuery()),
			new FakeIVideoProcess(), hashService);

		var (stream, resultModels) = await sut.GenerateThumbnail("/test.jpg",
			"hash", ThumbnailImageFormat.jpg, ThumbnailSize.Large);

		Assert.IsNull(stream);
		Assert.IsFalse(resultModels.Success);
	}

	[TestMethod]
	public async Task NotFoundNonExistingHash()
	{
		var sut = new ThumbnailService(new FakeSelectorStorage(),
			new FakeIWebLogger(), new AppSettings(),
			new UpdateStatusGeneratedThumbnailService(new FakeIThumbnailQuery()),
			new FakeIVideoProcess(),
			new FileHashSubPathStorage(new FakeSelectorStorage(), new FakeIWebLogger()));
		var result = await sut.GenerateThumbnail("/not-found", "non-existing-hash");
		Assert.IsFalse(result.FirstOrDefault()!.Success);
	}
}
