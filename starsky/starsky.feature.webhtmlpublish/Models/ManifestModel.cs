using System;
using System.Globalization;
using System.Reflection;
using starsky.foundation.platform.Models;

namespace starsky.feature.webhtmlpublish.Models
{
	public class ManifestModel
	{
		/// <summary>
		/// Display name
		/// </summary>
		public string Name { get; set; }
		
		/// <summary>
		/// To Identify the settings
		/// </summary>
		public string Key { get; set; }

		/// <summary>
		/// Slug, Name without spaces, but underscores are allowed
		/// </summary>
		public string Slug
		{
			get { return new AppSettings().GenerateSlug(Name, true); }
		}

		/// <summary>
		/// When did the export happen
		/// </summary>
		public string Export =>
			DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

		/// <summary>
		/// Starsky Version
		/// </summary>
		public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();
	}
}
