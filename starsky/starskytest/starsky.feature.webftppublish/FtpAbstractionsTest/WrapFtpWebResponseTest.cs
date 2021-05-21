using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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
			FtpWebResponse response = null;
			// ReSharper disable once ExpressionIsAlwaysNull
			new WrapFtpWebResponse(response).Dispose();
			Assert.IsNull(response);
			// nothing
		}
		
		[TestMethod]
		public void WrapFtpWebResponse_DisposeWithObject()
		{
			// https://stackoverflow.com/a/41961247
			var ctor =
				typeof(FtpWebResponse).GetConstructors(BindingFlags.Instance |
					BindingFlags.NonPublic | BindingFlags.InvokeMethod).FirstOrDefault();
			
			// using reflection
			// ReSharper disable once PossibleNullReferenceException
			var instance = (FtpWebResponse)ctor.Invoke(new object[]
			{
				new MemoryStream(), // Stream responseStream,
				1L, // long contentLength,
				new Uri("ftp://google.com"), // Uri responseUri,
				FtpStatusCode.AccountNeeded, //FtpStatusCode statusCode,
				string.Empty, // string statusLine,
				DateTime.Now, // DateTime lastModified,
				string.Empty, //  string bannerMessage,
				string.Empty, // string welcomeMessage
				string.Empty // string exitMessage
			});
			
			var response = new WrapFtpWebResponse(instance);
			response.Dispose();

			var responseStreamResult = response.GetResponseStream();
			Assert.AreEqual(Stream.Null, responseStreamResult);
		}

		[TestMethod]
		public void GetResponseStream()
		{
			var responseStreamResult =new WrapFtpWebResponse(null).GetResponseStream();
			Assert.AreEqual(Stream.Null, responseStreamResult);
		}
	}
}
