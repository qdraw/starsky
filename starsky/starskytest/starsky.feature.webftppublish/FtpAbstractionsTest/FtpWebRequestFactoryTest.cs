using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webftppublish.FtpAbstractions.Services;

namespace starskytest.starsky.feature.webftppublish.FtpAbstractionsTest;

[TestClass]
public sealed class FtpWebRequestFactoryTest
{
	[TestMethod]
	public void FtpWebRequestFactoryTestCreate_UriFormatException()
	{
		Assert.ThrowsExactly<UriFormatException>(() => new FtpWebRequestFactory().Create("t"));
		// Invalid URI: The format of the URI could not be determined.
	}

	[TestMethod]
	public void FtpWebRequestFactoryTestCreate_WebException()
	{
		var factory = new FtpWebRequestFactory();

		var test = factory.Create("ftp://test:test@404.undefined");
		Assert.ThrowsExactly<WebException>(() => test.GetResponse());
	}
}
