using System.Net.Http;
using System.Threading.Tasks;
using starskycore.Interfaces;

namespace starskycore.Helpers
{
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

		/// <summary>
		/// Get the Async results
		/// </summary>
		/// <param name="requestUri">https:// url</param>
		/// <returns>Task with Response</returns>
		public Task<HttpResponseMessage> GetAsync(string requestUri)
		{
			return _httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead);
		}

	}
}
