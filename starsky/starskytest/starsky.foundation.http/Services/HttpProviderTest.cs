using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

		Assert.AreEqual(HttpStatusCode.OK,result.Result.StatusCode);
	}
	
	[TestMethod]
	public void PostAsync_Ok_Form_xWwwHeader()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(httpClient);

		var request = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>());
		var result = httpProvider.PostAsync("http://test", request);

		Assert.AreEqual(HttpStatusCode.OK,result.Result.StatusCode);
		var contentTypeHeader = request.Headers
			.FirstOrDefault(p => p.Key == "Content-Type").Value.FirstOrDefault();
		Assert.AreEqual("application/x-www-form-urlencoded",contentTypeHeader);
	}
	
	[TestMethod]
	public void PostAsync_Null()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(httpClient);

		var result = httpProvider.PostAsync("http://test", null);

		Assert.AreEqual(HttpStatusCode.LoopDetected,result.Result.StatusCode);
	}
	
	[TestMethod]
	public void PostAsync_NotFound()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(httpClient);

		var result = httpProvider.PostAsync(
			"https://download.geonames.org", new StringContent(string.Empty));

		Assert.AreEqual(HttpStatusCode.NotFound,result.Result.StatusCode);
	}
}
