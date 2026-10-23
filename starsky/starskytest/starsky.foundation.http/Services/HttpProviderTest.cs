using System.Collections.Generic;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.http.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.http.Services;

[TestClass]
public sealed class HttpProviderTest
{
	[TestMethod]
	public async Task GetAsync_WithUserAgent_ShouldSendUserAgentHeader()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var result = await httpProvider.GetAsync("https://qdraw.nl/test", "Wget/1.21.1");

		Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
		var userAgent = string.Join(" ",
			fakeHttpMessageHandler.LastRequestMessage[0].Headers.UserAgent.Select(p => p.ToString()));
		Assert.Contains("Wget", userAgent);
	}

	[TestMethod]
	public async Task GetAsync_WithoutUserAgent_ShouldNotSendUserAgentHeader()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var result = await httpProvider.GetAsync("https://qdraw.nl/test");

		Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
		var userAgent = string.Join(" ",
			fakeHttpMessageHandler.LastRequestMessage[0].Headers.UserAgent.Select(p => p.ToString()));
		Assert.IsFalse(userAgent.Contains("Wget", StringComparison.Ordinal));
	}

	[TestMethod]
	public void PostAsync_Ok()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var result = httpProvider.PostAsync("http://test",
			new StringContent(string.Empty));

		Assert.AreEqual(HttpStatusCode.OK, result.Result.StatusCode);
	}

	[TestMethod]
	[Timeout(3000, CooperativeCancellation = true)]
	public void PostAsync_Ok_Form_xWwwHeader()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var request = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>());
		var result = httpProvider.PostAsync("http://test", request);

		Assert.AreEqual(HttpStatusCode.OK, result.Result.StatusCode);
		var contentTypeHeader = request.Headers
			.FirstOrDefault(p => p.Key == "Content-Type").Value.FirstOrDefault();
		Assert.AreEqual("application/x-www-form-urlencoded", contentTypeHeader);
	}

	[TestMethod]
	[Timeout(3000, CooperativeCancellation = true)]
	public async Task PostAsync_Null()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var result = await httpProvider.PostAsync("http://test", null);

		Assert.AreEqual(HttpStatusCode.LoopDetected, result.StatusCode);
	}

	[TestMethod]
	public async Task PostAsync_NotFound()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var result = await httpProvider.PostAsync(
			"https://download.geonames.org", new StringContent(string.Empty));

		Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
	}

	[TestMethod]
	public async Task PostAsync_WithAuthenticationHeaderValue_ShouldSendAuthHeader()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var result = await httpProvider.PostAsync(
			"http://test", new StringContent(string.Empty),
			new AuthenticationHeaderValue("Bearer", "test-token"));

		Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
		Assert.AreEqual("Bearer",
			fakeHttpMessageHandler.LastRequestMessage[0].Headers.Authorization?.Scheme);
		Assert.AreEqual("test-token",
			fakeHttpMessageHandler.LastRequestMessage[0].Headers.Authorization?.Parameter);
	}

	[TestMethod]
	public async Task PostAsync_WithUserAgent_ShouldSendUserAgentHeader()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var result = await httpProvider.PostAsync(
			"http://test", new StringContent(string.Empty), null, "Starsky-App");

		Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
		var userAgent = string.Join(" ",
			fakeHttpMessageHandler.LastRequestMessage[0].Headers.UserAgent.Select(p => p.ToString()));
		Assert.Contains("Starsky-App", userAgent);
	}

	[TestMethod]
	public async Task PostAsync_WithoutUserAgent_ShouldNotSendUserAgentHeader()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var result = await httpProvider.PostAsync(
			"http://test", new StringContent(string.Empty));

		Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
		var userAgent = string.Join(" ",
			fakeHttpMessageHandler.LastRequestMessage[0].Headers.UserAgent.Select(p => p.ToString()));
		Assert.IsFalse(userAgent.Contains("Starsky-App", StringComparison.Ordinal));
	}

	private static HttpRequestException TlsHandshakeException()
	{
		return new HttpRequestException("The SSL connection could not be established",
			new AuthenticationException("handshake failure"));
	}

	[TestMethod]
	public async Task PostAsync_TlsHandshakeFails_UsesCurlFallback()
	{
		var httpClient = new HttpClient(new FakeHttpMessageHandler(TlsHandshakeException()));
		var curl = new FakeICurlPostHelper(HttpStatusCode.OK, "from curl");
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient), curl);

		var result = await httpProvider.PostAsync("https://overpass-api.de/api/interpreter",
			new StringContent("data=test", System.Text.Encoding.UTF8,
				"application/x-www-form-urlencoded"), null, "Starsky-App");

		Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
		Assert.AreEqual("from curl", await result.Content.ReadAsStringAsync());
		Assert.HasCount(1, curl.Calls);
		Assert.AreEqual("https://overpass-api.de/api/interpreter", curl.Calls[0].RequestUri);
		Assert.AreEqual("data=test", curl.Calls[0].Body);
		Assert.Contains("application/x-www-form-urlencoded", curl.Calls[0].ContentType);
		Assert.AreEqual("Starsky-App", curl.Calls[0].UserAgent);
	}

	[TestMethod]
	public async Task PostAsync_TlsHandshakeFails_CurlNotAvailable_ThrowsOriginalException()
	{
		var httpClient = new HttpClient(new FakeHttpMessageHandler(TlsHandshakeException()));
		var curl = new FakeICurlPostHelper(null);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient), curl);

		var exception = await Assert.ThrowsExactlyAsync<HttpRequestException>(() =>
			httpProvider.PostAsync("https://overpass-api.de/api/interpreter",
				new StringContent("data=test")));

		Assert.IsInstanceOfType<AuthenticationException>(exception.InnerException);
		Assert.HasCount(1, curl.Calls);
	}

	[TestMethod]
	public async Task PostAsync_TlsHandshakeFails_WithAuthentication_DoesNotUseCurl()
	{
		var httpClient = new HttpClient(new FakeHttpMessageHandler(TlsHandshakeException()));
		var curl = new FakeICurlPostHelper();
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient), curl);

		await Assert.ThrowsExactlyAsync<HttpRequestException>(() =>
			httpProvider.PostAsync("https://qdraw.nl", new StringContent("data=test"),
				new AuthenticationHeaderValue("Bearer", "test-token")));

		Assert.IsEmpty(curl.Calls);
	}

	[TestMethod]
	public async Task PostAsync_OtherHttpRequestException_DoesNotUseCurl()
	{
		var httpClient = new HttpClient(
			new FakeHttpMessageHandler(new HttpRequestException("no route to host")));
		var curl = new FakeICurlPostHelper();
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient), curl);

		await Assert.ThrowsExactlyAsync<HttpRequestException>(() =>
			httpProvider.PostAsync("https://qdraw.nl", new StringContent("data=test")));

		Assert.IsEmpty(curl.Calls);
	}

	[TestMethod]
	public async Task PostAsync_TlsHandshakeFails_WithoutCurlHelper_Throws()
	{
		var httpClient = new HttpClient(new FakeHttpMessageHandler(TlsHandshakeException()));
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		await Assert.ThrowsExactlyAsync<HttpRequestException>(() =>
			httpProvider.PostAsync("https://qdraw.nl", new StringContent("data=test")));
	}
}
