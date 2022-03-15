using System;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.http.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.http.Services;

[TestClass]
public class HttpProviderTest
{
	[TestMethod]
	public void PostAsync_Ok()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(httpClient);

		var result = httpProvider.PostAsync("http://test", new StringContent(string.Empty));

		Assert.AreEqual(System.Net.HttpStatusCode.OK,result.Result.StatusCode);
	}
	[TestMethod]
	public void PostAsync_NotFound()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(httpClient);

		var result = httpProvider.PostAsync(
			"https://download.geonames.org", new StringContent(string.Empty));

		Assert.AreEqual(System.Net.HttpStatusCode.NotFound,result.Result.StatusCode);
	}
}
