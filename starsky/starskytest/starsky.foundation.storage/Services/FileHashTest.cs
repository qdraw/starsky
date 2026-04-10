using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnImageWithThumbnail;
using starskytest.FakeCreateAn.CreateAnQuickTimeMp4;
using starskytest.FakeMocks;
using VerifyMSTest;

namespace starskytest.starsky.foundation.storage.Services;

[TestClass]
public sealed class FileHashTest : VerifyBase
{
	[TestMethod]
	public void FileHashGenerateRandomBytesTest()
	{
		var input1 = FileHash.GenerateRandomBytes(10);
		var input2 = FileHash.GenerateRandomBytes(10);
		var test2 = FileHash.GenerateRandomBytes(0);
		Assert.HasCount(1, test2);
		Assert.AreNotEqual(input1, input2);
	}

	[TestMethod]
	public void FileHash_Md5TimeoutAsyncWrapper_Fail_Test()
	{
		// Give the hasher 0 seconds to calc a hash; so timeout is activated
		var iStorageFake = new FakeIStorage(
			["/"],
			["/test.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() }
		);
		var fileHashCode =
			new FileHash(iStorageFake, new FakeIWebLogger()).GetHashCode(
				"/test.jpg", ExtensionRolesHelper.ImageFormat.jpg, 0);

		Assert.IsFalse(fileHashCode.Value);
		Assert.Contains("_T", fileHashCode.Key);
	}

	[TestMethod]
	public async Task FileHash_CreateAnImage_Test_Verify()
	{
		var createAnImage = new CreateAnImage();
		var iStorage = new StorageSubPathFilesystem(
			new AppSettings { StorageFolder = createAnImage.BasePath }, new FakeIWebLogger());
		var fileHashCode =
			await new FileHash(iStorage, new FakeIWebLogger()).GetHashCodeAsync(
				createAnImage.DbPath,
				ExtensionRolesHelper.ImageFormat.jpg);
		Assert.IsTrue(fileHashCode.Value);
		Assert.AreEqual(26, fileHashCode.Key.Length);
		await Verify(fileHashCode);
	}

	[TestMethod]
	public async Task FileHash_CreateAnImageWithThumbnail_Test_Verify()
	{
		var createAnImage = new CreateAnImage();
		var iStorage = new FakeIStorage(
			["/"],
			[createAnImage.DbPath],
			new List<byte[]> { new CreateAnImageWithThumbnail().Bytes.ToArray() }
		);
		var fileHashCode =
			await new FileHash(iStorage, new FakeIWebLogger()).GetHashCodeAsync(
				createAnImage.DbPath,
				ExtensionRolesHelper.ImageFormat.jpg);
		Assert.IsTrue(fileHashCode.Value);
		Assert.AreEqual(26, fileHashCode.Key.Length);
		await Verify(fileHashCode);
	}

	[TestMethod]
	public async Task FileHash_StringArray_CreateAnImage_Test()
	{
		var createAnImage = new CreateAnImage();
		var iStorage = new StorageSubPathFilesystem(
			new AppSettings { StorageFolder = createAnImage.BasePath }, new FakeIWebLogger());
		var fileHashCode =
			new FileHash(iStorage, new FakeIWebLogger()).GetHashCode([
				createAnImage.DbPath
			]);
		Assert.IsTrue(fileHashCode[0].Value);
		Assert.AreEqual(26, fileHashCode[0].Key.Length);
		await Verify(fileHashCode);
	}

	[TestMethod]
	public async Task FileHash_StringArray_CreateAnQuickTimeMp4A6700_Test_Verify()
	{
		var createAnImage = new CreateAnImage();
		var iStorage = new FakeIStorage(
			["/"],
			[createAnImage.DbPath],
			new List<byte[]> { new CreateAnQuickTimeMp4A6700().Bytes.ToArray() }
		);
		var fileHashCode =
			new FileHash(iStorage, new FakeIWebLogger()).GetHashCode([
				createAnImage.DbPath
			]);
		Assert.IsTrue(fileHashCode[0].Value);
		Assert.AreEqual(26, fileHashCode[0].Key.Length);

		await Verify(fileHashCode);
	}

	[TestMethod]
	public async Task FileHash_StringArray_CreateAnQuickTimeMp4Wapp_Test_Verify()
	{
		var createAnImage = new CreateAnImage();
		var iStorage = new FakeIStorage(
			["/"],
			[createAnImage.DbPath],
			new List<byte[]> { new CreateAnQuickTimeMp4Wapp().Bytes.ToArray() }
		);
		var fileHashCode =
			new FileHash(iStorage, new FakeIWebLogger()).GetHashCode([
				createAnImage.DbPath
			]);
		Assert.IsTrue(fileHashCode[0].Value);
		Assert.AreEqual(26, fileHashCode[0].Key.Length);

		await Verify(fileHashCode);
	}


	[TestMethod]
	public async Task FileHash_StringArray_CreateAnQuickTimeMp4_Test_Verify()
	{
		var createAnImage = new CreateAnImage();
		var iStorage = new FakeIStorage(
			["/"],
			[createAnImage.DbPath],
			new List<byte[]> { CreateAnQuickTimeMp4.Bytes.ToArray() }
		);
		var fileHashCode =
			new FileHash(iStorage, new FakeIWebLogger()).GetHashCode([
				createAnImage.DbPath
			]);
		Assert.IsTrue(fileHashCode[0].Value);
		Assert.AreEqual(26, fileHashCode[0].Key.Length);
		Assert.AreEqual(CreateAnQuickTimeMp4.FileHash, fileHashCode[0].Key);

		await Verify(fileHashCode);
	}

	[TestMethod]
	public async Task FileHash_StringArray_CreateAnQuickTimeMp4_BytesWithLocation_Test_Verify()
	{
		var createAnImage = new CreateAnImage();
		var iStorage = new FakeIStorage(
			["/"],
			[createAnImage.DbPath],
			new List<byte[]> { CreateAnQuickTimeMp4.BytesWithLocation.ToArray() }
		);
		var fileHashCode =
			new FileHash(iStorage, new FakeIWebLogger()).GetHashCode([
				createAnImage.DbPath
			]);
		Assert.IsTrue(fileHashCode[0].Value);
		Assert.AreEqual(26, fileHashCode[0].Key.Length);
		Assert.AreEqual(CreateAnQuickTimeMp4.FileHashWithLocation, fileHashCode[0].Key);

		await Verify(fileHashCode);
	}

	[TestMethod]
	public async Task CalculateHashAsync_Dispose()
	{
		var stream = new MemoryStream();

		await FileHash.CalculateHashAsync(stream, cancellationToken: TestContext.CancellationToken);

		Assert.IsFalse(stream.CanSeek);
		Assert.IsFalse(stream.CanRead);
		Assert.IsFalse(stream.CanWrite);
	}
}
