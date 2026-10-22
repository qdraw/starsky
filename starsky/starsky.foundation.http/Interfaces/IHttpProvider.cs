using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace starsky.foundation.http.Interfaces;

public interface IHttpProvider
{
	/// <param name="followRedirects">false: the caller handles 3xx responses itself</param>
	Task<HttpResponseMessage> GetAsync(string requestUri, string? userAgent = null,
		bool followRedirects = true);

	Task<HttpResponseMessage> PostAsync(string requestUri,
		HttpContent? content, AuthenticationHeaderValue? authenticationHeaderValue = null);
}
