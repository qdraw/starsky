using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MetadataExtractor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.readmeta.ReadMetaHelpers;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;
using starsky.foundation.thumbnailgeneration.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnImageA6700PreviewRawJpeg;
using starskytest.FakeCreateAn.CreateAnImageEOS7DRawCr2;
using starskytest.FakeCreateAn.CreateAnImageEOSM50RawCr3;
using starskytest.FakeCreateAn.CreateAnQuickTimeMp4;
using starskytest.FakeMocks;
using VerifyMSTest;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory;

[TestClass]
public sealed class ThumbnailServiceTests : VerifyBase
{
	private readonly AppSettings _appSettings;
	private readonly string _fakeIStorageImageSubPath;
	private readonly string _fakeIStorageImageSubPathVideo;
	private readonly string _fakeIStorageRawArwImageSubPath;

	private readonly IUpdateStatusGeneratedThumbnailService
		_fakeIUpdateStatusGeneratedThumbnailService;

	private readonly ThumbnailImageFormat _imageFormat;

	private readonly FakeSelectorStorage _selectorStorage;
	private readonly string _fakeIStorageRawCr3ImageSubPath;
	private readonly string _fakeIStorageRawCr2ImageSubPath;

	public ThumbnailServiceTests()
	{
		_fakeIStorageImageSubPath = "/test.jpg";
		_fakeIStorageImageSubPathVideo = "/test.mp4";
		_fakeIStorageRawArwImageSubPath = "/test.arw";
		_fakeIStorageRawCr3ImageSubPath = "/test.cr3";
		_fakeIStorageRawCr2ImageSubPath = "/test.cr2";

		var iStorage = new FakeIStorage(["/"],
			[
				_fakeIStorageImageSubPath,
				_fakeIStorageImageSubPathVideo,
				_fakeIStorageRawArwImageSubPath,
				_fakeIStorageRawCr3ImageSubPath,
				_fakeIStorageRawCr2ImageSubPath
			],
			new List<byte[]>
			{
				CreateAnImage.Bytes.ToArray(),
				CreateAnQuickTimeMp4.Bytes.ToArray(),
				new CreateAnImageA6700PreviewRawJpeg().Bytes.ToArray(),
				new CreateAnImageEOSM50RawCr3().Bytes.ToArray(),
				new CreateAnImageEOS7DRawCr2().Bytes.ToArray()
			});
		_selectorStorage = new FakeSelectorStorage(iStorage);
		_appSettings = new AppSettings();
		_fakeIUpdateStatusGeneratedThumbnailService =
			new FakeIUpdateStatusGeneratedThumbnailService();
		_imageFormat = new AppSettings().ThumbnailImageFormat;
	}

	private ThumbnailService CreateSut(IStorage storage,
		IEmbeddedRawThumbnailService? service = null)
	{
		var selectorStorage = new FakeSelectorStorage(storage);
		return CreateSut(selectorStorage,
			new FileHashSubPathStorage(selectorStorage, new FakeIWebLogger()), service);
	}

	private ThumbnailService CreateSut(ISelectorStorage selectorStorage,
		IFileHashSubPathStorage hashService, IEmbeddedRawThumbnailService? service = null)
	{
		service ??= new FakeEmbeddedRawThumbnailService(selectorStorage);
		var thumbnailGeneratorFactory = new ThumbnailGeneratorFactory(selectorStorage,
			new FakeIWebLogger(),
			new FakeIVideoProcess(selectorStorage),
			new FakeINativePreviewThumbnailGenerator(),
			new EmbeddedRawThumbnailGenerator(selectorStorage,
				service,
				new FakeIWebLogger()));

		return new ThumbnailService(selectorStorage,
			new FakeIWebLogger(), _appSettings,
			_fakeIUpdateStatusGeneratedThumbnailService,
			hashService,
			thumbnailGeneratorFactory);
	}

	[TestMethod]
	public async Task GenerateThumbnail_FileHash_FileHashNull()
	{
		// Arrange
		var sut = CreateSut(new FakeIStorage());

		// Act
		var resultModels = await sut.GenerateThumbnail(
			"/not-found.jpg", null!);

		// Assert
		Assert.IsNotNull(resultModels);
		Assert.IsGreaterThanOrEqualTo(1, resultModels.Count);
		Assert.IsFalse(resultModels[0].Success);
	}

	[TestMethod]
	public async Task GenerateThumbnail_FileHash_ImageSubPathNotFound()
	{
		var sut = CreateSut(new FakeIStorage());

		var isCreated =
			await sut.GenerateThumbnail(
				"/not-found.jpg", _fakeIStorageImageSubPath);

		Assert.IsFalse(isCreated.FirstOrDefault()!.Success);
	}

	[TestMethod]
	public async Task GenerateThumbnail_FileHash_WrongImageType()
	{
		var sut = CreateSut(new FakeIStorage());

		var isCreated = await sut.GenerateThumbnail(
			"/notfound.dng", _fakeIStorageImageSubPath);

		Assert.IsFalse(isCreated.FirstOrDefault()!.Success);
	}

	[TestMethod]
	public async Task GenerateThumbnail_FileHash_WrongImageType_Verify()
	{
		var sut = CreateSut(new FakeIStorage());

		var isCreated = await sut.GenerateThumbnail(
			"/notfound.dng", _fakeIStorageImageSubPath);

		await Verify(isCreated);
	}

	private static async Task Verify(List<GenerationResultModel> result)
	{
		await Verifier.Verify(result).DontScrubDateTimes();
	}

	[TestMethod]
	public async Task GenerateThumbnail_FileHash_Video_HappyFlow()
	{
		var sut = CreateSut(_selectorStorage.Get(SelectorStorage.StorageServices.SubPath));

		var isCreated = await sut.GenerateThumbnail(
			_fakeIStorageImageSubPathVideo);

		Assert.IsTrue(isCreated[0].Success);
		Assert.IsTrue(isCreated[1].Success);
		Assert.IsTrue(isCreated[2].Success);
	}

	[TestMethod]
	public async Task GenerateThumbnail_FileHash_RawArw_HappyFlow()
	{
		var sut = CreateSut(
			_selectorStorage.Get(SelectorStorage.StorageServices.SubPath),
			new EmbeddedRawThumbnailService(new FakeIWebLogger(), _selectorStorage));

		var isCreated = await sut.GenerateThumbnail(
			_fakeIStorageRawArwImageSubPath);

		Assert.IsTrue(isCreated[0].Success);
		Assert.IsTrue(isCreated[1].Success);
		Assert.IsTrue(isCreated[2].Success);

		await AssertDecode();
	}

	[TestMethod]
	public async Task GenerateThumbnail_FileHash_RawCr3_HappyFlow()
	{
		var sut = CreateSut(
			_selectorStorage.Get(SelectorStorage.StorageServices.SubPath),
			new EmbeddedRawThumbnailService(new FakeIWebLogger(), _selectorStorage));

		var isCreated = await sut.GenerateThumbnail(
			_fakeIStorageRawCr3ImageSubPath);

		Assert.IsTrue(isCreated[0].Success);
		Assert.IsTrue(isCreated[1].Success);
		Assert.IsTrue(isCreated[2].Success);

		await AssertDecode();
	}
	
	[TestMethod]
	public async Task GenerateThumbnail_FileHash_RawCr2_HappyFlow()
	{
		var sut = CreateSut(
			_selectorStorage.Get(SelectorStorage.StorageServices.SubPath),
			new EmbeddedRawThumbnailService(new FakeIWebLogger(), _selectorStorage));

		var isCreated = await sut.GenerateThumbnail(
			_fakeIStorageRawCr2ImageSubPath);

		Assert.IsTrue(isCreated[0].Success);
		Assert.IsTrue(isCreated[1].Success);
		Assert.IsTrue(isCreated[2].Success);

		await AssertDecode();
	}

	private async Task AssertDecode()
	{
		var imageHelper = new ResizeThumbnailFromSourceImageHelper(
			_selectorStorage,
			new FakeIWebLogger());

		var output = Guid.NewGuid().ToString();

		// Ensure the preview.jpg exists in temporary storage
		var tempStorage = _selectorStorage.Get(SelectorStorage.StorageServices.Temporary);
		if ( !tempStorage.ExistFile($"preview_{output}.jpg") )
		{
			await tempStorage.WriteStreamAsync(new MemoryStream([.. CreateAnImage.Bytes]),
				$"preview_{output}.jpg");
		}

		await imageHelper.ResizeThumbnailFromSourceImage(
			$"preview_{output}.jpg",
			SelectorStorage.StorageServices.Temporary,
			1000, output,
			true, ThumbnailImageFormat.jpg);

		var stream = _selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail)
			.ReadStream($"{output}.jpg");
		var meta = ImageMetadataReader.ReadMetadata(stream).ToList();
		await stream.DisposeAsync();

		tempStorage.FileDelete($"preview_{output}.jpg");
		tempStorage.FileDelete($"{output}.jpg");

		Assert.AreEqual(1000, ReadMetaExif.GetImageWidthHeight(meta, true));
		Assert.AreEqual(667, ReadMetaExif.GetImageWidthHeight(meta, false));
	}

	[TestMethod]
	public async Task GenerateThumbnail_FileHash_Video_ProcessFailed()
	{
		var sut = CreateSut(new FakeIStorage());

		var isCreated = await sut.GenerateThumbnail(
			_fakeIStorageImageSubPathVideo);

		Assert.IsFalse(isCreated[0].Success);
		Assert.IsFalse(isCreated[1].Success);
		Assert.IsFalse(isCreated[2].Success);
	}

	[TestMethod]
	[DataRow(ThumbnailGenerationType.All)]
	[DataRow(ThumbnailGenerationType.SkipExtraLarge)]
	public async Task GenerateThumbnail_FileHash_IncludeOrSkipExtraLarge(
		ThumbnailGenerationType type)
	{
		var storage = new FakeIStorage(["/"],
			["/test.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		const string fileHash = "test_hash";
		var selectorStorage = new FakeSelectorStorage(storage);
		var sut = CreateSut(selectorStorage,
			new FileHashSubPathStorage(selectorStorage, new FakeIWebLogger()));

		var isCreated =
			await sut.GenerateThumbnail("/test.jpg", fileHash, type);

		Assert.IsNotNull(isCreated);
		Assert.IsGreaterThanOrEqualTo(1, isCreated.Count);
		Assert.IsTrue(isCreated.FirstOrDefault()!.Success);
		var thumbStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		Assert.IsTrue(thumbStorage.ExistFile(
			ThumbnailNameHelper.Combine(fileHash, ThumbnailSize.Large, _imageFormat)));
		Assert.IsTrue(thumbStorage.ExistFile(
			ThumbnailNameHelper.Combine(fileHash, ThumbnailSize.Small, _imageFormat)));

		// depend on includeExtraLarge
		Assert.AreEqual(type != ThumbnailGenerationType.SkipExtraLarge, thumbStorage.ExistFile(
			ThumbnailNameHelper.Combine(fileHash, ThumbnailSize.ExtraLarge, _imageFormat)));
	}

	[TestMethod]
	public async Task GenerateThumbnail_1arg_ThumbnailAlreadyExist()
	{
		var storage = new FakeIStorage(["/"],
			["/test.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var hash =
			( await new FileHash(storage, new FakeIWebLogger()).GetHashCodeAsync(
				"/test.jpg",
				ExtensionRolesHelper.ImageFormat.jpg) )
			.Key;

		var selectorStorage = new FakeSelectorStorage(storage);
		var thumbStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);

		await thumbStorage.WriteStreamAsync(
			StringToStreamHelper.StringToStream("not 0 bytes"),
			ThumbnailNameHelper.Combine(hash, ThumbnailSize.ExtraLarge, _imageFormat));
		await thumbStorage.WriteStreamAsync(
			StringToStreamHelper.StringToStream("not 0 bytes"),
			ThumbnailNameHelper.Combine(hash, ThumbnailSize.Large, _imageFormat));
		await thumbStorage.WriteStreamAsync(
			StringToStreamHelper.StringToStream("not 0 bytes"),
			ThumbnailNameHelper.Combine(hash, ThumbnailSize.Small, _imageFormat));

		var sut = CreateSut(selectorStorage,
			new FileHashSubPathStorage(selectorStorage, new FakeIWebLogger()));

		var isCreated =
			await sut.GenerateThumbnail("/test.jpg"); // 1 arg

		Assert.IsGreaterThanOrEqualTo(1, isCreated.Count, "Results should not be empty");
		Assert.IsTrue(isCreated[0].Success);
	}

	[TestMethod]
	public async Task GenerateThumbnail_1arg_Folder()
	{
		var storage = new FakeIStorage(["/"],
			["/test.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var sut = CreateSut(storage);
		var isCreated = await sut.GenerateThumbnail("/");

		Assert.IsGreaterThanOrEqualTo(1, isCreated.Count, "Results should not be empty");
		Assert.IsTrue(isCreated[0].Success);
	}

	[TestMethod]
	public async Task GenerateThumbnail_1arg_FolderWithNoSupportedFiles_ReturnsEmpty()
	{
		var storage = new FakeIStorage(["/"],
			["/notes.txt"],
			new List<byte[]> { "hello"u8.ToArray() });

		var sut = CreateSut(storage);
		var result = await sut.GenerateThumbnail("/");

		Assert.IsNotNull(result);
		Assert.IsEmpty(result);
	}

	[TestMethod]
	public async Task GenerateThumbnail_NullFail()
	{
		var storage = new FakeIStorage(["/test"],
			["/test/test.jpg"],
			new List<byte[]?> { null });

		var sut = CreateSut(storage);

		var isCreated = await sut.GenerateThumbnail("/test/test.jpg");

		Assert.IsEmpty(isCreated);
	}

	[TestMethod]
	public async Task GenerateThumbnail__CorruptJpeg_Verify()
	{
		var storage = new FakeIStorage(
			["/"],
			["/test.jpg"],
			new List<byte[]> { Array.Empty<byte>() });

		var sut = CreateSut(storage);

		var result = await sut.GenerateThumbnail("/test.jpg");

		Assert.IsNotNull(result);
		Assert.IsGreaterThanOrEqualTo(1, result.Count);
	}

	[TestMethod]
	public async Task GenerateThumbnail__CorruptDng_Verify()
	{
		var storage = new FakeIStorage(
			["/"],
			["/test.dng"],
			new List<byte[]> { Array.Empty<byte>() });

		var sut = CreateSut(storage);

		var result = await sut.GenerateThumbnail(
			"/test.dng");

		Assert.IsNotNull(result);
		Assert.IsGreaterThanOrEqualTo(1, result.Count);
	}

	[TestMethod]
	public async Task RotateThumbnail_NotFound()
	{
		var sut = CreateSut(new FakeIStorage());

		var result = await sut.RotateThumbnail("not-found", 0, 3);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task RotateThumbnail_Rotate()
	{
		var storage = new FakeIStorage(
			["/"],
			["/test.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var sut = CreateSut(storage);
		var result = await sut.RotateThumbnail("/test.jpg", -1, 3);

		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task RotateThumbnail_Corrupt()
	{
		var storage = new FakeIStorage(
			["/"],
			["test"],
			new List<byte[]> { Array.Empty<byte>() });

		var sut = CreateSut(storage);

		var result = await sut.RotateThumbnail("/test.jpg", -1, 3);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task GenerateThumbnail_NotFound()
	{
		var sut = CreateSut(new FakeIStorage());
		var resultModels = await sut.GenerateThumbnail("/not-found");

		Assert.IsFalse(resultModels.FirstOrDefault()!.Success);
	}

	[TestMethod]
	public async Task GenerateThumbnail_NotFound2()
	{
		var sut = CreateSut(new FakeIStorage());
		var (stream, resultModels) = await sut.GenerateThumbnail("/not-found",
			"hash", ThumbnailImageFormat.unknown, ThumbnailSize.Large);

		Assert.IsNull(stream);
		Assert.IsFalse(resultModels.Success);
	}

	[TestMethod]
	public async Task GenerateThumbnail_NotFound3()
	{
		var storage = new FakeSelectorStorage(new FakeIStorage([],
			[], [[.. CreateAnImage.Bytes]]));

		var hashService = new FakeIFileHashSubPathStorage([( "/test.jpg", "hash", false )]);
		var sut = CreateSut(storage, hashService);

		var (stream, resultModels) = await sut.GenerateThumbnail("/test.jpg",
			"hash", ThumbnailImageFormat.jpg, ThumbnailSize.Large);

		Assert.IsNull(stream);
		Assert.IsFalse(resultModels.Success);
	}

	[TestMethod]
	public async Task GenerateThumbnail_InvalidFileHash()
	{
		var storage = new FakeSelectorStorage(new FakeIStorage([],
			["/test.jpg"], [[.. CreateAnImage.Bytes]]));

		var hashService = new FakeIFileHashSubPathStorage([( "/test.jpg", "hash", false )]);

		var sut = CreateSut(storage, hashService);

		var (stream, resultModels) = await sut.GenerateThumbnail("/test.jpg",
			"hash", ThumbnailImageFormat.jpg, ThumbnailSize.Large);

		Assert.IsNull(stream);
		Assert.IsFalse(resultModels.Success);
		Assert.IsTrue(resultModels.ErrorMessage?.Contains("Invalid fileHash"));
	}

	[TestMethod]
	public async Task GenerateThumbnail_NotFoundNonExistingHash()
	{
		var sut = CreateSut(new FakeIStorage());
		var result = await sut.GenerateThumbnail("/not-found", "non-existing-hash");
		Assert.IsFalse(result.FirstOrDefault()!.Success);
	}
}
