#nullable enable
using System.Net.Http;
using System.Threading.Tasks;

namespace starsky.foundation.http.Interfaces
{
	public interface IHttpProvider
	{
		Task<HttpResponseMessage> GetAsync(string requestUri);

		Task<HttpResponseMessage> PostAsync(string requestUri,
			HttpContent? content);
	}
}
