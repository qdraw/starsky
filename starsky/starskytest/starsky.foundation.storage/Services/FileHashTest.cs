using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.Services;

[TestClass]
public sealed class FileHashTest
{
	[TestMethod]
	public void FileHashGenerateRandomBytesTest()
	{
		var input1 = FileHash.GenerateRandomBytes(10);
		var input2 = FileHash.GenerateRandomBytes(10);
		var test2 = FileHash.GenerateRandomBytes(0);
		Assert.AreEqual(1, test2.Length);
		Assert.AreNotEqual(input1, input2);
	}

	[TestMethod]
	public void FileHash_Md5TimeoutAsyncWrapper_Fail_Test()
	{
		// Give the hasher 0 seconds to calc a hash; so timeout is activated
		var iStorageFake = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() }
		);
		var fileHashCode =
			new FileHash(iStorageFake, new FakeIWebLogger()).GetHashCode("/test.jpg", 0);

		Assert.IsFalse(fileHashCode.Value);
		Assert.IsTrue(fileHashCode.Key.Contains("_T"));
	}

	[TestMethod]
	public void FileHash_CreateAnImage_Test()
	{
		var createAnImage = new CreateAnImage();
		var iStorage = new StorageSubPathFilesystem(
			new AppSettings { StorageFolder = createAnImage.BasePath }, new FakeIWebLogger());
		var fileHashCode =
			new FileHash(iStorage, new FakeIWebLogger()).GetHashCode(createAnImage.DbPath);
		Assert.IsTrue(fileHashCode.Value);
		Assert.AreEqual(26, fileHashCode.Key.Length);
	}

	[TestMethod]
	public void FileHash_StringArray_CreateAnImage_Test()
	{
		var createAnImage = new CreateAnImage();
		var iStorage = new StorageSubPathFilesystem(
			new AppSettings { StorageFolder = createAnImage.BasePath }, new FakeIWebLogger());
		var fileHashCode =
			new FileHash(iStorage, new FakeIWebLogger()).GetHashCode(new[]
			{
				createAnImage.DbPath
			});
		Assert.IsTrue(fileHashCode[0].Value);
		Assert.AreEqual(26, fileHashCode[0].Key.Length);
	}

	[TestMethod]
	public async Task CalculateHashAsync_Dispose()
	{
		var stream = new MemoryStream();

		await FileHash.CalculateHashAsync(stream);

		Assert.IsFalse(stream.CanSeek);
		Assert.IsFalse(stream.CanRead);
		Assert.IsFalse(stream.CanWrite);
	}
}
