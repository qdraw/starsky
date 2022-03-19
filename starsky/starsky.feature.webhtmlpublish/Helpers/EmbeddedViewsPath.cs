using System;
using System.IO;

namespace starsky.feature.webhtmlpublish.Helpers
{
	public static class EmbeddedViewsPath
	{
		public static string GetViewFullPath(string viewName)
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
