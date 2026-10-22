using System.Net.Http;

namespace starskytest.FakeMocks;

public class FakeIHttpClientFactory : IHttpClientFactory
{
	private readonly HttpClient _httpClient;

	public FakeIHttpClientFactory(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public HttpClient CreateClient(string name) => _httpClient;
}
