#nullable enable
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using starsky.foundation.http.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeIHttpProvider : IHttpProvider
	{
		public List<string> UrlCalled = new List<string>();
		
		private readonly Dictionary<string, HttpContent> _inputDictionary;
		
		public FakeIHttpProvider(Dictionary<string,HttpContent>? inputDictionary = null)
		{
			if ( inputDictionary == null ) inputDictionary = new Dictionary<string, HttpContent>();
			_inputDictionary = inputDictionary;
		}
		
		public Task<HttpResponseMessage> GetAsync(string requestUri)
		{
			UrlCalled.Add(requestUri);

			if ( !_inputDictionary.ContainsKey(requestUri) )
			{
				return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound){
					Content = new StringContent("Not Found")
				});
			}

			var response =
				new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = _inputDictionary[requestUri]
				};
			return Task.FromResult(response);
		}

		public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent? content)
		{
			return await GetAsync(requestUri);
		}
	}
}
