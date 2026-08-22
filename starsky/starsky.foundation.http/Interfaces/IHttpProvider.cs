using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace starsky.foundation.http.Interfaces;

public interface IHttpProvider
{
	Task<HttpResponseMessage> GetAsync(string requestUri, string? userAgent = null);

	Task<HttpResponseMessage> PostAsync(string requestUri,
		HttpContent? content, AuthenticationHeaderValue? authenticationHeaderValue = null);
}
