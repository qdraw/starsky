using System;
using System.IO;

namespace starskywebhtmlcli.Services
{
	public class EmbeddedViewsPath
	{
		public string GetViewFullPath(string viewName)
		{
			return AppDomain.CurrentDomain.BaseDirectory +
			       Path.DirectorySeparatorChar +
			       "WebHtmlPublish" +
			       Path.DirectorySeparatorChar +
			       "EmbeddedViews" +
			       Path.DirectorySeparatorChar + viewName;
		}
	}
}
