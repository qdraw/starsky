using System.Net;
using starsky.feature.webftppublish.FtpAbstractions.Helpers;
using starsky.feature.webftppublish.FtpAbstractions.Interfaces;
using starsky.foundation.injection;
#pragma warning disable SYSLIB0014

namespace starsky.feature.webftppublish.FtpAbstractions.Services
{
	/// <summary>
	/// Abstract the Response and Request
	/// @see: https://stackoverflow.com/a/9823224
	/// </summary>
	[Service(typeof(IFtpWebRequestFactory), InjectionLifetime = InjectionLifetime.Scoped)]
	public class FtpWebRequestFactory : IFtpWebRequestFactory
	{
		/// <summary>
		/// (FtpWebRequest)WebRequest is deprecated
		/// </summary>
		/// <param name="uri">ftp url</param>
		/// <returns>new Requester</returns>
		public IFtpWebRequest Create(string uri)
		{
			return new WrapFtpWebRequest((FtpWebRequest)WebRequest.Create(uri));
		}
	}
}
