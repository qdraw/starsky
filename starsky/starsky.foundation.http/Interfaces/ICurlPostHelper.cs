using System.Net.Http;
using System.Threading.Tasks;

namespace starsky.foundation.http.Interfaces;

/// <summary>
///     POST with the curl command line tool, used when .NET can not set up the TLS connection.
///     .NET on macOS does not support TLS 1.3 and some servers (Overpass) no longer accept TLS 1.2
/// </summary>
public interface ICurlPostHelper
{
	/// <summary>
	///     Post a body to a https url
	/// </summary>
	/// <param name="requestUri">https:// url</param>
	/// <param name="body">text body</param>
	/// <param name="contentType">Content-Type header</param>
	/// <param name="userAgent">optional User-Agent</param>
	/// <returns>null when curl is not available or failed to connect</returns>
	Task<HttpResponseMessage?> PostAsync(string requestUri, string body, string contentType,
		string? userAgent);
}
