using System.Collections.Generic;
using System.Threading.Tasks;

namespace starsky.foundation.http.Interfaces
{
	public interface IHttpClientHelper
	{
		Task<bool> Download(string sourceHttpUrl, string fullLocalPath);
		Task<KeyValuePair<bool, string>> ReadString(string sourceHttpUrl);
	}
}
