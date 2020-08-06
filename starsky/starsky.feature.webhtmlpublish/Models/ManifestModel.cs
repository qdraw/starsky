using System;
using System.Collections.Generic;
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
		/// Which files or folders need to be copied
		/// </summary>
		public List<string> Copy { get; set; } = new List<string>();

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
