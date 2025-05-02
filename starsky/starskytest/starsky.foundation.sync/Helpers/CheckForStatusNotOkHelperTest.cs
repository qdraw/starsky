using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.sync.Helpers;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.Helpers;

[TestClass]
public class CheckForStatusNotOkHelperTest
{
	[TestMethod]
	public void CheckForStatusNotOk_CorruptCheck()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { Array.Empty<byte>() });

		var item = new CheckForStatusNotOkHelper(storage, new FakeIWebLogger())
			.CheckForStatusNotOk("/test.jpg").FirstOrDefault();
		Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, item?.Status);
	}

	[TestMethod]
	public void CheckForStatusNotOk_ValidImage()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImageNoExif.Bytes.ToArray() });

		var item = new CheckForStatusNotOkHelper(storage, new FakeIWebLogger())
			.CheckForStatusNotOk("/test.jpg").FirstOrDefault();
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, item?.Status);
	}

	[TestMethod]
	public void CheckForStatusNotOk_DifferentTypeNotSupported()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.43758" },
			new List<byte[]> { CreateAnImageNoExif.Bytes.ToArray() });

		var sut = new CheckForStatusNotOkHelper(storage, new FakeIWebLogger());
		var item = sut.CheckForStatusNotOk("/test.43758").FirstOrDefault();
		Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, item?.Status);
	}

	[TestMethod]
	public void CheckForStatusNotOk_NotFound()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImageNoExif.Bytes.ToArray() });

		var sut = new CheckForStatusNotOkHelper(storage, new FakeIWebLogger());

		var item = sut.CheckForStatusNotOk("/not-found.jpg")
			.FirstOrDefault();
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, item?.Status);
	}
}
