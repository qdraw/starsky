using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using starskycore.Interfaces;

namespace starskycore.Helpers
{
	public class HttpProvider : IHttpProvider
	{
		private readonly HttpClient _httpClient;

		public HttpProvider(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public Task<HttpResponseMessage> GetAsync(string requestUri)
		{
			return _httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead);
		}


	}
}
