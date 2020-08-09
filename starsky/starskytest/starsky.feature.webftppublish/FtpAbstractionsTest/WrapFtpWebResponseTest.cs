using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webftppublish.FtpAbstractions.Helpers;

namespace starskytest.starsky.feature.webftppublish.FtpAbstractionsTest
{
	[TestClass]
	public class WrapFtpWebResponseTest
	{
		[TestMethod]
		public void WrapFtpWebResponse1_Dispose()
		{
			new WrapFtpWebResponse(null).Dispose();
			// nothing
		}

		[TestMethod]
		[ExpectedException(typeof(System.NullReferenceException))]
		public void GetResponseStream()
		{
			new WrapFtpWebResponse(null).GetResponseStream();
			// NullReferenceException
		}
	}
}
