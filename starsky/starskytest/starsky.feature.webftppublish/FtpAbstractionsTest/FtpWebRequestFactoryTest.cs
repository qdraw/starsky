using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webftppublish.FtpAbstractions.Services;

namespace starskytest.starsky.feature.webftppublish.FtpAbstractionsTest
{
	[TestClass]
	public class FtpWebRequestFactoryTest
	{
		[TestMethod]
		[ExpectedException(typeof(System.UriFormatException))]
		public void FtpWebRequestFactoryTestCreate_UriFormatException()
		{
			new FtpWebRequestFactory().Create("t");
			// Invalid URI: The format of the URI could not be determined.
		}
		
		[TestMethod]
		[ExpectedException(typeof(System.Net.WebException))]
		public void FtpWebRequestFactoryTestCreate_WebException()
		{
			var factory = new FtpWebRequestFactory();

			var test = factory.Create("ftp://test:test@404.nl");
			test.GetResponse();
		}
	}
}
