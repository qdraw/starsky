using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace starsky.foundation.http.Interfaces;

public interface IHttpClientHelper
{
	/// <summary>
	///     Download a file
	/// </summary>
	/// <param name="restricted">
	///     use for user supplied urls: redirects are followed manually (max 3 hops, allowlist
	///     is checked on every hop) and the size is limited to <c>MaxRestrictedDownloadBytes</c>
	/// </param>
	Task<bool> Download(Uri sourceUri, string fullLocalPath, int retryAfterInSeconds = 15,
		string? userAgent = null, bool restricted = false);

	Task<bool> Download(string sourceHttpUrl, string fullLocalPath, int retryAfterInSeconds = 15,
		string? userAgent = null, bool restricted = false);
	Task<KeyValuePair<bool, string>> ReadString(string sourceHttpUrl);
	Task<KeyValuePair<bool, string>> ReadString(Uri sourceHttpUrl);

	Task<KeyValuePair<bool, string>> PostString(string sourceHttpUrl,
		HttpContent? httpContent, AuthenticationHeaderValue? authenticationHeaderValue = null,
		bool verbose = true);
}
