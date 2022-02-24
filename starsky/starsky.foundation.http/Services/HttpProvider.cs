#nullable enable
using System.Net.Http;
using System.Threading.Tasks;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;

namespace starsky.foundation.http.Services
{
	[Service(typeof(IHttpProvider), InjectionLifetime = InjectionLifetime.Singleton)]
	public class HttpProvider : IHttpProvider
	{
		/// <summary>
		/// HttpClient object
		/// </summary>
		private readonly HttpClient _httpClient;

		/// <summary>
		/// Inject http client
		/// </summary>
		/// <param name="httpClient">c# http client</param>
		public HttpProvider(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		private const string UserAgent =
			"Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";

		/// <summary>
		/// Get the Async results
		/// </summary>
		/// <param name="requestUri">https:// url</param>
		/// <returns>Task with Response</returns>
		public Task<HttpResponseMessage> GetAsync(string requestUri)
		{
			_httpClient.DefaultRequestHeaders.Add("User-Agent",UserAgent);
			return _httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead);
		}

		/// <summary>
		/// Post the Async results
		/// </summary>
		/// <param name="requestUri">https:// url</param>
		/// <param name="content">http content</param>
		/// <returns>Task with Response</returns>
		public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent? content)
		{
			_httpClient.DefaultRequestHeaders.Add("User-Agent",UserAgent);
			return _httpClient.PostAsync(requestUri, content);
		}
	}
}
