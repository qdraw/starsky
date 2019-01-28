using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Helpers;
using starskytests.FakeMocks;

namespace starskytests.Helpers
{
	[TestClass]
	public class HttpClientHelperTest
	{
		[TestMethod]
		public void HttpClientHelperT()
		{
			var http = new FakeHttpMessageHandler();
			var httpProvider = new HttpClient(http);
			var t = new HttpProvider(httpProvider);

			var t = new HttpClientHelper(httpProvider);
			//HttpMessageHandler
		}
	}
}
