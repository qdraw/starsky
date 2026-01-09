using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;

namespace starsky.foundation.http.Services;

[Service(typeof(IHttpProvider), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class HttpProvider : IHttpProvider
{
	private const string UserAgent =
		"Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";

	/// <summary>
	///     HttpClient object
	/// </summary>
	private readonly HttpClient _httpClient;

	/// <summary>
	///     Inject http client
	/// </summary>
	/// <param name="httpClient">c# http client</param>
	public HttpProvider(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <summary>
	///     Get the Async results
	/// </summary>
	/// <param name="requestUri">https:// url</param>
	/// <returns>Task with Response</returns>
	public Task<HttpResponseMessage> GetAsync(string requestUri)
	{
		_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UserAgent);
		return _httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead);
	}

	/// <summary>
	///     Post the Async results
	/// </summary>
	/// <param name="requestUri">https:// url</param>
	/// <param name="content">http content</param>
	/// <param name="authenticationHeaderValue"></param>
	/// <returns>Task with Response</returns>
	public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent? content,
		AuthenticationHeaderValue? authenticationHeaderValue = null)
	{
		if ( content == null )
		{
			return Task.FromResult(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.LoopDetected,
				Content = new StringContent("http content is null")
			});
		}

		_httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);

		var request = new HttpRequestMessage
		{
			Method = HttpMethod.Post, Content = content, RequestUri = new Uri(requestUri)
		};

		if ( authenticationHeaderValue != null )
		{
			request.Headers.Authorization = authenticationHeaderValue;
		}

		if ( typeof(FormUrlEncodedContent) == content.GetType() )
		{
			request.Headers.TryAddWithoutValidation("Content-Type",
				"application/x-www-form-urlencoded");
		}

		return _httpClient.SendAsync(request);
	}
}
