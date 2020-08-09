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
		[ExpectedException(typeof(System.InvalidCastException))]
		public void FtpWebRequestFactoryTestCreate_InvalidCastException()
		{
			var factory = new FtpWebRequestFactory();

			factory.Create("/");
			// System.InvalidCastException:
		}
	}
}
