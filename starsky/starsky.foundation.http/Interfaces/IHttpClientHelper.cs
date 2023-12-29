#nullable enable
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace starsky.foundation.http.Interfaces
{
	public interface IHttpClientHelper
	{
		Task<bool> Download(string sourceHttpUrl, string fullLocalPath, int retryAfterInSeconds = 15);
		
		/// <summary>
		/// Get String of webPage - does check with domain whitelist
		/// </summary>
		/// <param name="sourceHttpUrl">webUrl</param>
		/// <returns>bool: success or fail and string content of result</returns>
		Task<KeyValuePair<bool, string>> ReadString(string sourceHttpUrl);
		
		Task<KeyValuePair<bool, string>> PostString(string sourceHttpUrl,
			HttpContent? httpContent, bool verbose = true);
	}
}
