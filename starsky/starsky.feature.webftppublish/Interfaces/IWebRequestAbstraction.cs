using System.Net;

namespace starsky.feature.webftppublish.Interfaces
{
	public interface IWebRequestAbstraction
	{
		WebRequest Create(string requestUriString);
	}
}
