using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webftppublish.FtpAbstractions;

namespace starskytest.starsky.feature.webftppublish.FtpAbstractionsTest
{
	[TestClass]
	public class WrapFtpWebRequestTest
	{
		// Use abstraction
		// These are all null because WebRequest has no public ctor
		
		[TestMethod]
		[ExpectedException(typeof(System.NullReferenceException))]
		public void Method_Null()
		{
			new WrapFtpWebRequest(null) {Method = "t"};
		}
		
		[TestMethod]
		[ExpectedException(typeof(System.NullReferenceException))]
		public void Credentials_Null()
		{
			new WrapFtpWebRequest(null) {Credentials = new NetworkCredential()};
		}
		
		[TestMethod]
		[ExpectedException(typeof(System.NullReferenceException))]
		public void UsePassive_Null()
		{
			new WrapFtpWebRequest(null) {UsePassive = true};
		}
		
		[TestMethod]
		[ExpectedException(typeof(System.NullReferenceException))]
		public void UseBinary_Null()
		{
			new WrapFtpWebRequest(null) {UseBinary = true};
		}
		
		[TestMethod]
		[ExpectedException(typeof(System.NullReferenceException))]
		public void KeepAlive_Null()
		{
			new WrapFtpWebRequest(null) {KeepAlive = true};
		}
		
		[TestMethod]
		[ExpectedException(typeof(System.NullReferenceException))]
		public void GetResponse_Null()
		{
			new WrapFtpWebRequest(null).GetResponse();
		}
	}
}
