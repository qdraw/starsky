using System.Net;
using starsky.feature.webftppublish.Interfaces;
using starsky.foundation.injection;

namespace starsky.feature.webftppublish.Services
{
	[Service(typeof(IWebRequestAbstraction), InjectionLifetime = InjectionLifetime.Scoped)]
	public class WebRequestAbstraction : IWebRequestAbstraction
	{
		public WebRequest Create(string requestUriString)
		{
			return WebRequest.Create(requestUriString);
		}
	}
}
