using System;
using System.Globalization;
using System.Reflection;
using starsky.foundation.platform.Models;

namespace starsky.feature.webhtmlpublish.Models
{
	public class ManifestModel
	{
		public string Name { get; set; }

		public string Slug
		{
			get { return new AppSettings().GenerateSlug(Name, true); }
		}

		public string Export =>
			DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

		public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();
	}
}
