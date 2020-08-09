using System.Net;
using starsky.feature.webftppublish.FtpAbstractions.Helpers;
using starsky.feature.webftppublish.FtpAbstractions.Interfaces;
using starsky.foundation.injection;

namespace starsky.feature.webftppublish.FtpAbstractions.Services
{
	/// <summary>
	/// @see: https://stackoverflow.com/a/9823224
	/// </summary>
	[Service(typeof(IFtpWebRequestFactory), InjectionLifetime = InjectionLifetime.Scoped)]
	public class FtpWebRequestFactory : IFtpWebRequestFactory
	{
		public IFtpWebRequest Create(string uri)
		{
			return new WrapFtpWebRequest((FtpWebRequest)WebRequest.Create(uri));
		}
	}
}
