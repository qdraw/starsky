using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using starsky.foundation.http.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeIHttpProvider : IHttpProvider
	{
		private readonly Dictionary<string, HttpContent> _inputDictionary;
		
		public FakeIHttpProvider(Dictionary<string,HttpContent> inputDictionary)
		{
			_inputDictionary = inputDictionary;
		}
		
#pragma warning disable 1998
		public async Task<HttpResponseMessage> GetAsync(string requestUri)
#pragma warning restore 1998
		{
			if ( !_inputDictionary.ContainsKey(requestUri) )
			{
				return new HttpResponseMessage(HttpStatusCode.NotFound);
			}

			var response =
				new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = _inputDictionary[requestUri]
				};
			return response;
		}
	}
}
