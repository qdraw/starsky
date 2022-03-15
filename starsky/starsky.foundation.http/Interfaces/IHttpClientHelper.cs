#nullable enable
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace starsky.foundation.http.Interfaces
{
	public interface IHttpClientHelper
	{
		Task<bool> Download(string sourceHttpUrl, string fullLocalPath);
		Task<KeyValuePair<bool, string>> ReadString(string sourceHttpUrl);
		Task<KeyValuePair<bool, string>> PostString(string sourceHttpUrl,
			HttpContent? httpContent, bool verbose = true);
	}
}
