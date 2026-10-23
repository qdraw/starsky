using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using starsky.foundation.http.Interfaces;

namespace starskytest.FakeMocks;

public class FakeICurlPostHelper(HttpStatusCode? statusCode = HttpStatusCode.OK,
	string responseBody = "curl response") : ICurlPostHelper
{
	public List<(string RequestUri, string Body, string ContentType, string? UserAgent)> Calls
	{
		get;
	} = [];

	public Task<HttpResponseMessage?> PostAsync(string requestUri, string body,
		string contentType, string? userAgent)
	{
		Calls.Add((requestUri, body, contentType, userAgent));
		if ( statusCode == null )
		{
			return Task.FromResult<HttpResponseMessage?>(null);
		}

		return Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage(statusCode.Value)
		{
			Content = new StringContent(responseBody)
		});
	}
}
