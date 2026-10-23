using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading.Tasks;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;

namespace starsky.foundation.http.Services;

[Service(typeof(IHttpProvider), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class HttpProvider : IHttpProvider
{
	internal const string HttpClientName = "starsky";

	/// <summary>
	///     Same as <see cref="HttpClientName" /> but without automatic redirects
	/// </summary>
	internal const string NoRedirectHttpClientName = "starsky-no-redirect";

	private readonly ICurlPostHelper? _curlPostHelper;
	private readonly IHttpClientFactory _httpClientFactory;

	/// <summary>
	///     Inject http client factory
	/// </summary>
	/// <param name="httpClientFactory">IHttpClientFactory</param>
	/// <param name="curlPostHelper">used when the TLS handshake of .NET fails</param>
	public HttpProvider(IHttpClientFactory httpClientFactory,
		ICurlPostHelper? curlPostHelper = null)
	{
		_httpClientFactory = httpClientFactory;
		_curlPostHelper = curlPostHelper;
	}

	/// <summary>
	///     Get the Async results
	/// </summary>
	/// <param name="requestUri">https:// url</param>
	/// <returns>Task with Response</returns>
	public Task<HttpResponseMessage> GetAsync(string requestUri, string? userAgent = null,
		bool followRedirects = true)
	{
		var clientName = followRedirects ? HttpClientName : NoRedirectHttpClientName;
		if ( string.IsNullOrWhiteSpace(userAgent) )
		{
			return _httpClientFactory.CreateClient(clientName)
				.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead);
		}

		var request = new HttpRequestMessage
		{
			Method = HttpMethod.Get, RequestUri = new Uri(requestUri)
		};
		request.Headers.TryAddWithoutValidation("User-Agent", userAgent);

		return _httpClientFactory.CreateClient(clientName)
			.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
	}

	/// <summary>
	///     Post the Async results
	/// </summary>
	/// <param name="requestUri">https:// url</param>
	/// <param name="content">http content</param>
	/// <param name="authenticationHeaderValue"></param>
	/// <returns>Task with Response</returns>
	public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent? content,
		AuthenticationHeaderValue? authenticationHeaderValue = null, string? userAgent = null)
	{
		if ( content == null )
		{
			return new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.LoopDetected,
				Content = new StringContent("http content is null")
			};
		}

		var request = new HttpRequestMessage
		{
			Method = HttpMethod.Post, Content = content, RequestUri = new Uri(requestUri)
		};

		if ( authenticationHeaderValue != null )
		{
			request.Headers.Authorization = authenticationHeaderValue;
		}

		if ( !string.IsNullOrWhiteSpace(userAgent) )
		{
			request.Headers.TryAddWithoutValidation("User-Agent", userAgent);
		}

		if ( typeof(FormUrlEncodedContent) == content.GetType() )
		{
			request.Headers.TryAddWithoutValidation("Content-Type",
				"application/x-www-form-urlencoded");
		}

		try
		{
			return await _httpClientFactory.CreateClient(HttpClientName).SendAsync(request);
		}
		catch ( HttpRequestException exception ) when ( exception.InnerException is
			                                              AuthenticationException &&
		                                              _curlPostHelper != null &&
		                                              authenticationHeaderValue == null )
		{
			// .NET on macOS has no TLS 1.3, curl has. Not used with credentials,
			// as arguments of a process are visible to other users
			var curlResponse = await _curlPostHelper.PostAsync(requestUri,
				await content.ReadAsStringAsync(),
				content.Headers.ContentType?.ToString() ?? "text/plain", userAgent);
			if ( curlResponse == null )
			{
				throw;
			}

			return curlResponse;
		}
	}
}
