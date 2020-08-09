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
		public void Method_Set_Null()
		{
			new WrapFtpWebRequest(null) {Method = "t"};
		}
		
		[TestMethod]
		[ExpectedException(typeof(System.NullReferenceException))]
		public void Method_Get_Null()
		{
			var result = new WrapFtpWebRequest(null).Method;
		}
		
		[TestMethod]
		[ExpectedException(typeof(System.NullReferenceException))]
		public void Credentials_Set_Null()
		{
			new WrapFtpWebRequest(null) {Credentials = new NetworkCredential()};
		}
		
		[TestMethod]
		public void Credentials_Get_Null()
		{
			// your not allowed to get creds
			var result = new WrapFtpWebRequest(null).Credentials;
			Assert.IsNull(result);
		}
		
		[TestMethod]
		[ExpectedException(typeof(System.NullReferenceException))]
		public void UsePassive_Set_Null()
		{
			new WrapFtpWebRequest(null) {UsePassive = true};
		}
		
		[TestMethod]
		[ExpectedException(typeof(System.NullReferenceException))]
		public void UsePassive_Get_Null()
		{
			var result = new WrapFtpWebRequest(null).UsePassive;
		}
		
		[TestMethod]
		[ExpectedException(typeof(System.NullReferenceException))]
		public void UseBinary_Set_Null()
		{
			new WrapFtpWebRequest(null) {UseBinary = true};
		}
		
		[TestMethod]
		[ExpectedException(typeof(System.NullReferenceException))]
		public void KeepAlive_Get_UseBinary()
		{
			var result = new WrapFtpWebRequest(null).UseBinary;
		}
		
		[TestMethod]
		[ExpectedException(typeof(System.NullReferenceException))]
		public void KeepAlive_Set_Null()
		{
			new WrapFtpWebRequest(null) {KeepAlive = true};
		}
		
		[TestMethod]
		[ExpectedException(typeof(System.NullReferenceException))]
		public void KeepAlive_Get_Null()
		{
			var keepAlive = new WrapFtpWebRequest(null).KeepAlive;
		}
		
		[TestMethod]
		[ExpectedException(typeof(System.NullReferenceException))]
		public void GetResponse_Get_Null()
		{
			new WrapFtpWebRequest(null).GetResponse();
		}
		
		[TestMethod]
		[ExpectedException(typeof(System.NullReferenceException))]
		public void GetRequestStream_Get_Null()
		{
			new WrapFtpWebRequest(null).GetRequestStream();
		}
	}
}
