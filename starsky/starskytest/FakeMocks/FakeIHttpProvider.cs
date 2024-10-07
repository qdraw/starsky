using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using starsky.foundation.http.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIHttpProvider : IHttpProvider
{
	private readonly Dictionary<string, HttpContent> _inputDictionary;
	public List<string> UrlCalled = new();

	public FakeIHttpProvider(Dictionary<string, HttpContent>? inputDictionary = null)
	{
		inputDictionary ??= new Dictionary<string, HttpContent>();

		_inputDictionary = inputDictionary;
	}

	public Task<HttpResponseMessage> GetAsync(string requestUri)
	{
		UrlCalled.Add(requestUri);

		if ( !_inputDictionary.TryGetValue(requestUri, out var value) )
		{
			return Task.FromResult(
				new HttpResponseMessage(HttpStatusCode.NotFound)
				{
					Content = new StringContent("Not Found")
				});
		}

		var response =
			new HttpResponseMessage(HttpStatusCode.OK) { Content = value };
		return Task.FromResult(response);
	}

	public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent? content)
	{
		return await GetAsync(requestUri);
	}
}
