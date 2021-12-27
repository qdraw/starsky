using System.Linq;
using System.Net;

namespace starsky.foundation.accountmanagement.Helpers
{
	public static class IsLocalhost
	{
		/// <summary>
		/// Check if current request is from localhost
		/// @see: https://stackoverflow.com/a/44775206
		/// </summary>
		/// <param name="connectionLocalIpAddress">context.Connection.LocalIpAddress</param>
		/// <param name="connectionRemoteIpAddress"></param>
		/// <returns></returns>
		public static bool IsHostLocalHost(IPAddress connectionLocalIpAddress, IPAddress connectionRemoteIpAddress)
		{
			if ( connectionLocalIpAddress == null || connectionRemoteIpAddress == null ) return false;
			return connectionRemoteIpAddress.Equals(connectionLocalIpAddress) || IPAddress.IsLoopback(connectionRemoteIpAddress);
		}
	}
}
