using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Helpers;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.Helpers;

[TestClass]
public class CheckSha256HelperTest
{
	private readonly FakeIStorage _fakeIStorage = new(new List<string> { "/" },
		["/exiftool.exe"],
		[[.. CreateAnExifToolTarGz.Bytes]]);

	[TestMethod]
	public void CheckSha256_Good()
	{
		var sut = new CheckSha256Helper(_fakeIStorage);
		var result2 = sut.CheckSha256("/exiftool.exe",
			[CreateAnExifToolTarGz.Sha256]);
		Assert.IsTrue(result2);
	}

	[TestMethod]
	public void CheckSha1_Bad()
	{
		var sut = new CheckSha256Helper(_fakeIStorage);
		var result2 = sut.CheckSha256("/exiftool.exe",
			["random_value"]);
		Assert.IsFalse(result2);
	}
}
