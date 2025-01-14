using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Shared;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Testers;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.Shared;

[TestClass]
public class RemoveCorruptThumbnailTests
{
	private const string FakeIStorageImageSubPath = "/test.jpg";

	[TestMethod]
	public async Task WriteErrorMessageToBlockLog()
	{
		var storage = new FakeIStorage(["/"],
			[FakeIStorageImageSubPath],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var sut = new RemoveCorruptThumbnail(new FakeSelectorStorage(storage));

		await sut.WriteErrorMessageToBlockLog(FakeIStorageImageSubPath, "fail");

		var logPath = ErrorLogItemFullPath.GetErrorLogItemFullPath(FakeIStorageImageSubPath);

		Assert.IsTrue(storage.ExistFile(logPath));
	}

	[TestMethod]
	public void RemoveIfCorrupt_NotExist()
	{
		var storage = new FakeIStorage(
			["/"],
			[],
			[]);

		var sut = new RemoveCorruptThumbnail(new FakeSelectorStorage(storage));
		var result = sut.RemoveIfCorrupt("test");

		Assert.IsNull(result);
	}

	[TestMethod]
	public void RemoveIfCorrupt_Ignore()
	{
		var storage = new FakeIStorage(["/"],
			[FakeIStorageImageSubPath],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var sut = new RemoveCorruptThumbnail(new FakeSelectorStorage(storage));

		sut.RemoveIfCorrupt(FakeIStorageImageSubPath);

		Assert.IsTrue(storage.ExistFile(FakeIStorageImageSubPath));
	}

	[TestMethod]
	public void RemoveIfCorrupt_IsRemoved()
	{
		byte[] emptyArray = [];
		var storage = new FakeIStorage(["/"],
			[FakeIStorageImageSubPath],
			new List<byte[]> { emptyArray });

		var sut = new RemoveCorruptThumbnail(new FakeSelectorStorage(storage));
		sut.RemoveIfCorrupt(FakeIStorageImageSubPath);

		Assert.IsFalse(storage.ExistFile(FakeIStorageImageSubPath));
	}
}
